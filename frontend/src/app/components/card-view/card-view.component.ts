import { Component, Inject } from "@angular/core";
import { FormBuilder, FormControl, FormGroup } from "@angular/forms";
import { MatChipInputEvent } from "@angular/material/chips";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material/dialog";
import { Card } from "src/app/models/card";
import { Post } from "src/app/models/post";
import { Tag } from "src/app/models/tag";
import { User } from "src/app/models/user";
import { UserCard } from "src/app/models/userCard";
import { CardService } from "src/app/services/card.service";
import { PostService } from "src/app/services/post.service";
import { TagService } from "src/app/services/tag.service";
import { UserCardService } from "src/app/services/usercard.service";

@Component({
    selector: 'card-view',
    templateUrl: './card-view.component.html',
    styleUrls: ['./card-view.component.css']
})
export class CardViewComponent {
    private card: Card;
    private editCard: boolean;
    private usersOnTheCard: User[] = new Array();
    private linkedToBoardUsers: User[] = new Array();
    private posts: Post[] = new Array();
    private postspinner = true;
    private tagsspinner = true;
    private tags: Tag[] = new Array();
    public frm: FormGroup;
    public ctlPseudo: FormControl;

    constructor(public dialogRef: MatDialogRef<CardViewComponent>,
        @Inject(MAT_DIALOG_DATA) public data: { card: Card; linkedToBoardUsers: User[] },
        private cardService: CardService,
        private postService: PostService,
        private usercardService : UserCardService,
        private tagService : TagService,
        private fb: FormBuilder
    ) {
        this.card = data.card;
        this.refreshTags();
        this.usercardService.getRelationUsersCard(this.card.id).subscribe(
            res => {
                this.usersOnTheCard = res;
                this.linkedToBoardUsers = data.linkedToBoardUsers;
                this.linkedToBoardUsers = this.linkedToBoardUsers.filter(el => !this.usersOnTheCard.find(u => (u.id === el.id)));
            }
        );
        this.frm = this.fb.group({
        });
        this.frm.patchValue(data.card);
        this.refreshPosts();
    }

    displayEditCard() {
        return this.editCard;
    }

    toggleDisplayEditCard() {
        this.editCard = !this.editCard;
    }

    onEnterForEditCard(value: string, cardId: number) {
        this.editCard = !this.editCard;
        this.cardService.getById(cardId).subscribe(res => {
            res.name = value;
            this.cardService.update(res).subscribe(r => {
                let timeout: NodeJS.Timer;
                timeout = setTimeout(() => {
                    this.refreshDialog();
                }, 100);
            });
        });
    }

    refreshDialog() {
        this.cardService.getById(this.card.id).subscribe(
            res => {
                this.card = res;
            }
        )
    }

    refreshUpdateDialog() {
        this.cardService.getById(this.card.id).subscribe(res => {
            this.cardService.update(res).subscribe(r => {
                let timeout: NodeJS.Timer;
                timeout = setTimeout(() => {
                    this.refreshDialog();
                }, 100);
            });
        });
    }

    deleteCard() {
        this.cardService.delete(this.card.id).subscribe(
            res => {
                this.close();
            }
        );
    }

    deletePost(postId: number){
      this.postService.delete(postId).subscribe(res => {
        this.refreshPosts();
      });
    }

    close() {
        this.dialogRef.close();
    }

    addUserToCard(u: User) {
        this.usercardService.addUserCardRelation(new UserCard({Card : this.card, User : u}))
        .subscribe(
            res => {
                this.usercardService.getRelationUsersCard(this.card.id).subscribe(
                    res => {
                        this.usersOnTheCard = res;
                        this.linkedToBoardUsers = this.linkedToBoardUsers.filter(el => !this.usersOnTheCard.find(u => (u.id === el.id)));
                        this.refreshUpdateDialog();
                    }
                );
            }
        )
    }

    removeUserFromCard(u: User) {
        this.usercardService.removeUserCardRelation(new UserCard({Card : this.card, User : u}))
        .subscribe(
            res => {
                this.usercardService.getRelationUsersCard(this.card.id).subscribe(
                    res => {
                        this.usersOnTheCard = res;
                        this.linkedToBoardUsers.push(u);
                        this.refreshUpdateDialog();
                    }
                );
            }
        )
    }

    onEnterForPostText(value: string) {
        console.log(value);

        const c = new Post(null);
        c.text = value;
        c.card = this.card
        this.postService.add(c).subscribe(
            res => {
                this.refreshPosts();
            }
        );
      }

      fileChangeI(event) {
        const fileList: FileList = event.target.files;
        if (fileList.length > 0) {
            const file = fileList[0];
            this.postService.uploadPicture(this.card.name || 'empty', file).subscribe(path => {
                this.frm.markAsDirty();

                const c = new Post(null);
                c.picturePath = path;
                c.card = this.card;
                this.postService.add(c).subscribe(
                    res => {
                        this.refreshPosts();
                    }
                );
            });
        }
    }

    refreshPosts(){
        this.postService.getAllbyCard(this.card.id).subscribe(
            res => {
                console.log("POSTS")
                console.log(res);
                if (res) {
                    this.postspinner = false;
                }
                this.posts = res;
            }
        );
    }
    addTag(event: MatChipInputEvent): void {
        const input = event.input;
        const value = event.value;

        var t = new Tag(null);
        t.name = value;
        this.tagService.add(t, this.card.id).subscribe(
            res => {
                this.refreshTags();
            }
        )
        if (input) {
          input.value = '';
        }
      }

    deleteTag(t:Tag){
        this.tagService.delete(t.id).subscribe(
            res => {
                this.refreshTags();
            }
        )
    }

    refreshTags(){
        this.tagService.getAllbyCard(this.card.id).subscribe(
            r => {
                this.tags = r;
                this.tagsspinner = false;
            }
        )
    }
}
