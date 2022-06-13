import { UserBoard } from './../../models/userBoard';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { BoardList } from 'src/app/models/boardList';
import { CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { CardService } from 'src/app/services/card.service';
import { Card } from 'src/app/models/card';
import { MatDialog } from '@angular/material/dialog';
import { AddBoardListComponent } from '../add-boardlist/add-boardlist.component';
import { BoardListService } from '../../services/boardlist.service';
import * as _ from 'lodash';
import { BoardService } from 'src/app/services/board.service';
import { Board } from 'src/app/models/board';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CardViewComponent } from '../card-view/card-view.component';
import { User } from 'src/app/models/user';
import { UserBoardService } from 'src/app/services/userboard.service';
import { GeneralHubService } from 'src/app/services/general-hub.service';
import { AuthenticationService } from 'src/app/services/authentication.service';

@Component({
  selector: 'app-board-view',
  templateUrl: './board-view.component.html',
  styleUrls: ['./board-view.component.css']
})
export class BoardViewComponent implements OnInit, OnDestroy {
  public boardId: number;
  public board: Board;
  public boardLists: BoardList[];
  public usersLinkedToBoard: User[] = new Array(); // List utilisée pour la card-view
  public usersBoardRelationList: UserBoard[]; // List contenant tous les utilisateurs aujoutés ou non à la board
  public sub;
  mapCards: Map<number, boolean> = new Map<number, boolean>();
  mapBoardLists: Map<number, boolean> = new Map<number, boolean>();
  displayEditBoard: boolean;
  displayRight = false;

  constructor(
    private boardListService: BoardListService,
    private cardService: CardService,
    private boardService: BoardService,
    private userboardService: UserBoardService,
    private route: ActivatedRoute, private router: Router, public dialog: MatDialog,
    public snackBar: MatSnackBar, private ghs: GeneralHubService, private auth: AuthenticationService) { }

  ngOnInit(): void {
    this.hubConfiguration();
    this.sub = this.route.params.subscribe(params => {
      this.boardId = params['id'];
      this.refreshBoard();
      this.refreshBoardLists();
      this.refreshUserBoard();
    });
  }

  ngOnDestroy() {
    this.sub.unsubscribe();
  }

  hubConfiguration() {
    this.ghs.newData.subscribe(res => {
      // Ces conditions servent à ne rafraichir que les pages nécessaires
      // Gère la reception de donnée de type "boardList" ou "Card"
      if (res.classType === 'BoardList' || res.classType === 'Card') {
        if (this.boardLists.length > 0) {
          this.boardLists.forEach(bl => {
            // Recheche si la "boardList" reçue à le même Id qu'une présente dans la vue.
            // Ou recherche si la "boardList" reçue se retrouve bien dans la "board".
            // Ou recherche si la "card" reçue se trouve dans la une "boardList".
            // Dans tous ces cas on met à jour la board-view.
            if (bl.id === res.id || bl.id === res.boardListId || this.boardId == res.boardId) {
              let timeout: NodeJS.Timer;
              timeout = setTimeout(() => {
                this.refreshBoardLists();
              }, 500);
              return;
            }
          });
        } else {
          if (this.boardId == res.boardId) {
            let timeout: NodeJS.Timer;
            timeout = setTimeout(() => {
              this.refreshBoardLists();
            }, 500);
          }
        }
      } else if (res.classType === 'UserBoard') {
        if (this.boardId == res.boardId) {
          if (res.deleted && res.user.pseudo == this.auth.currentUser.pseudo) {
            this.router.navigate(['/all-boards']);
          } else {
            let timeout: NodeJS.Timer;
            timeout = setTimeout(() => {
              this.refreshUserBoard();
            }, 500);
          }
        }
      } else if (res.classType === 'Board') {
        if (this.boardId == res.id) {
          if (res.deleted && this.auth.currentUser.pseudo != res.owner.pseudo) {
            this.router.navigate(['/all-boards']);
          } else if (!res.deleted) {
            let currentUrl = this.router.url;
            this.router.navigateByUrl('/', { skipLocationChange: true }).then(() => {
              this.router.navigate([currentUrl]);
            });
          }
        }
      } else if (res.classType == 'User' && res.pseudo == this.board.owner.pseudo) {
        this.router.navigate(['/all-boards']);
        // Cette dernière condition devrait être améliorer !
      } else if (res.classType == 'User' && res.deleted) {
        let timeout: NodeJS.Timer;
        timeout = setTimeout(() => {
          this.refreshUserBoard();
          this.refreshBoardLists();
        }, 500);
      }
    });
  }

  dropGroup(event: CdkDragDrop<string[]>) {
    moveItemInArray(this.boardLists, event.previousIndex, event.currentIndex);
  }

  dropItem(event: CdkDragDrop<string[]>) {
    if (event.previousContainer === event.container) {
      this.cardService.getById(event.item.data['id']).subscribe(
        res => {
          this.cardService.updatePosition(res, event.currentIndex, +event.container['id']).subscribe();
        }
      );
      moveItemInArray(
        event.container.data,
        event.previousIndex,
        event.currentIndex
      );
    } else {
      this.cardService.getById(event.item.data['id']).subscribe(
        res => {
          this.cardService.updatePosition(res, event.currentIndex, +event.container['id']).subscribe();
        }
      );
      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex
      );
    }
  }

  getConnectedList(): any[] {
    return this.boardLists.map(x => `${x.id}`);
  }

  toggleDisplayEditBoard() {
    this.displayEditBoard = !this.displayEditBoard;
  }

  toggleDisplayEditList(blId: number) {
    if (!this.mapBoardLists.get(blId)) {
      this.boardLists.forEach(element => {
        this.mapCards.set(element.id, false);
        this.mapBoardLists.set(element.id, false);
      });
    }
    this.mapBoardLists.set(blId, !this.mapBoardLists.get(blId));
  }

  displayEditList(blId: number) {
    return this.mapBoardLists.get(blId);
  }

  toggleDisplayAddCard(blId: number) {
    if (!this.mapCards.get(blId)) {
      this.boardLists.forEach(element => {
        this.mapCards.set(element.id, false);
        this.mapBoardLists.set(element.id, false);
      });
    }
    this.mapCards.set(blId, !this.mapCards.get(blId));
  }

  //
  displayAddCard(blId: number) {
    return this.mapCards.get(blId);
  }

  // Gère l'appuie sur la touche enter lorsqu'on édite une "board"
  onEnterForEditBoard(value: string) {
    this.boardService.update(this.board, value).subscribe(res => {
      if (!res) {
        this.snackBar.open(`There was an error at the server. It's possible that you choose a similar name for an already existing board or that the length of the name is less than 3 or greater than 25! Please try again with other name or contact support.`,
          'Dismiss', { duration: 10000 });
        this.toggleDisplayEditBoard();
      }
    });
  }

  // Gère l'appuie sur la touche enter lorsqu'on édite une "boardList"
  onEnterForBoardList(newName: string, boardListId: number) {
    this.mapBoardLists.set(boardListId, !this.mapBoardLists.get(boardListId));
    this.boardListService.update(_.find(this.boardLists, e => e.id === boardListId), newName, this.boardId).subscribe(res => {
      if (!res) {
        this.snackBar.open(`There was an error at the server. It's possible that you choose a similar name for an already existing card or that the length of the name is less than 3 or greater than 15! Please try again with other name or contact support.`,
          'Dismiss', { duration: 10000 });
      }
    });
  }

  // Gère l'appuie sur la touche enter lorsqu'on édite une "card"
  onEnterForCard(value: string, listId: number) {
    this.mapCards.set(listId, !this.mapCards.get(listId));
    const c = new Card(null);
    c.name = value;
    this.cardService.add(c, listId).subscribe(
      res => {
        if (!res) {
          this.snackBar.open(`There was an error at the server. It's possible that you choose a similar name for a card of in diffentes list or that the length of the name is less than 3 or greater than 15! Please try again with other name or contact support.`,
            'Dismiss', { duration: 10000 });
        }
      }
    );
  }

  addBoardList() {
    const dlg = this.dialog.open(AddBoardListComponent, {panelClass: 'cardviewdialog'});
    dlg.beforeClosed().subscribe(res => {
      if (res) {
        this.boardListService.add(this.boardId, res).subscribe(res => {
          if (!res) {
            this.snackBar.open(`There was an error at the server. It's possible you choose a similar name of board list! Please try again with other name or contact support.`,
              'Dismiss', { duration: 10000 });
          }
        }
        )
      }
    });
  }

  deleteBoardList(n: number) {
    const snackBarRef = this.snackBar.open(`The table list will be deleted`, 'Undo', { duration: 10000 });
    snackBarRef.afterDismissed().subscribe(res => {
      if (!res.dismissedByAction) {
        this.boardListService.delete(n).subscribe();
      }
    })
  }

  // Ouvre la card-view
  open(card: Card) {

    const linkedToBoardUsers = this.usersLinkedToBoard.filter(u => u.pseudo != card.owner.pseudo);

    const dlg = this.dialog.open(CardViewComponent, { data: { card, linkedToBoardUsers }, panelClass: 'cardviewdialog' });
    dlg.afterClosed().subscribe(res => {
      if (res) {
        this.cardService.update(res).subscribe();
      }
    });
  }

  openRightMenu() {
    this.displayRight = true;
  }

  closeRightMenu() {
    this.displayRight = false;
  }

  removeUserBoardRelation(userBoard: UserBoard) {
    this.userboardService.removeUserBoardRelation(userBoard).subscribe();
  }

  addUserBoardRelation(userBoard: UserBoard) {
    this.userboardService.addUserBoardRelation(userBoard).subscribe();
  }

  // Remplit la liste des utilisateurs liés à la "board"
  setLinkedUsers(data: UserBoard[]) {
    this.usersLinkedToBoard = []; // Pour vider l'array
    data.forEach(e => {
      if (e.userIsInvitedOnTheBoard) {
        this.usersLinkedToBoard.push(e.user);
      }
    });
    this.usersLinkedToBoard.push(this.board.owner);// Ajout l'owner de la board dans la liste
  }

  // Rafraîchessement de la "board"
  refreshBoard() {
    this.boardService.getById(this.boardId).subscribe(
      res => {
        this.board = res;
      }
    );
  }

  // Rafraîchessement des "BoardLists"
  refreshBoardLists() {
    this.boardListService.getAllByBoardId(this.boardId).subscribe(
      lists => {
        this.boardLists = lists;
      }
    );
  }

  // Rafraîchessement des relations "users" "board"
  refreshUserBoard() {
    this.userboardService.getRelationUsersBoard(this.boardId).subscribe(
      res => {
        this.setLinkedUsers(res);
        this.usersBoardRelationList = res;
      }
    );
  }
}
