import { FormBuilder, FormGroup, FormArray, FormControl } from '@angular/forms';
import { AfterViewInit, Component } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Board } from 'src/app/models/board';

import { BoardService } from '../../services/board.service';
import { AddBoardComponent } from '../add-board/add-board.component';
import * as _ from 'lodash';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { GeneralHubService } from 'src/app/services/general-hub.service';

@Component({
  selector: 'app-boards',
  templateUrl: './board.component.html',
  styleUrls: ['./board.component.css']
})
export class BoardComponent implements AfterViewInit {

  public userBoards: Board[];
  public boardsWhereUserIsInvited: Board[];
  public slctBoards: boolean;
  formBoards: FormGroup;
  checkArray: FormArray;

  constructor(private router: Router, private boardService: BoardService,
    public dialog: MatDialog, private fb: FormBuilder, public snackBar: MatSnackBar, private ghs: GeneralHubService) {
    this.formBoards = this.fb.group({
      checkArray: this.fb.array([])
    });
  }

  ngAfterViewInit(): void {
    // Rafraîchis toutes les pages car tous les utilisateurs sont concernés par l'invitation sur une board.
    this.ghs.newData.subscribe(res => {
      if (res.classType === 'UserBoard') { this.refreshBoardsWhereUserIsInvited(); }
    });
    this.boardService.getAllBoardsFromAUser().subscribe(
      boards => {
        this.userBoards = boards;
      }
    );
    this.boardService.getAllBoardsWhereUserIsInvited().subscribe(
      boards => {
        this.boardsWhereUserIsInvited = boards;
      }
    );
    this.checkArray = this.formBoards.get('checkArray') as FormArray;
  }

  addBoard() {
    const dlg = this.dialog.open(AddBoardComponent, {panelClass: 'cardviewdialog'});
    dlg.beforeClosed().subscribe(res => {
      if (res) {
        this.boardService.add(res).subscribe(res => {
          if (!res) {
            this.snackBar.open(`There was an error at the server. It's possible that you choose a similar name for an already existing board or that the length of the name is less than 3 or greater than 25! Please try again with other name or contact support.`,
              'Dismiss', { duration: 10000 });
          } else {
            let timeout: NodeJS.Timer;
            timeout = setTimeout(() => { this.refreshUserBoards(); }, 500);
          }
        });
      }
    });
  }

  selectBoards() {
    this.slctBoards ? this.slctBoards = false : this.slctBoards = true;
  }

  cancelDeleteBoards() {
    this.slctBoards = false;
  }

  deleteBoards() {
    this.slctBoards = false;
    if (this.formBoards.get('checkArray').value.length > 0) {
      const snackBarRef = this.snackBar.open(`One or more tables will be deleted`, 'Undo', { duration: 10000 });
      snackBarRef.afterDismissed().subscribe(res => {
        if (!res.dismissedByAction) {
          this.boardService.delete(this.formBoards.get('checkArray').value).subscribe(res => {
            if (res) {
              let timeout: NodeJS.Timer;
              timeout = setTimeout(() => {
                this.router.navigate(['/boards']);
                this.refreshUserBoards();
              }, 500);
            }
          });
        }
        this.checkArray.clear();
      })
    }
  }

  onCheckboxChange(boardId: number) {
    const index = _.findIndex(this.checkArray.value, (n) => n === boardId);
    if (index > -1) {
      this.checkArray.removeAt(index);
    } else {
      this.checkArray.push(new FormControl(boardId));
    }
  }

  refreshUserBoards() {
    this.boardService.getAllBoardsFromAUser().subscribe(
      boards => {
        this.userBoards = boards;
      }
    );
  }

  refreshBoardsWhereUserIsInvited() {
    this.boardService.getAllBoardsWhereUserIsInvited().subscribe(
      boards => {
        this.boardsWhereUserIsInvited = boards;
      }
    );
  }
}
