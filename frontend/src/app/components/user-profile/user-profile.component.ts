import { Component, OnInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { User } from 'src/app/models/user';
import { AuthenticationService } from 'src/app/services/authentication.service';
import { UserService } from '../../services/user.service';

@Component({
    selector: 'app-userprofile',
    templateUrl: './user-profile.component.html',
    styleUrls: ['./user-profile.component.css']
})
export class UserProfileComponent{
    user: User;
    userLoaded: Promise<boolean>;

    constructor(private userService: UserService, private auth: AuthenticationService, private router: Router,  public snackBar: MatSnackBar){
      this.userService.getByPseudo(this.auth.currentUser.pseudo).subscribe(res => {
        this.user = res;
        this.userLoaded = Promise.resolve(true);
      })
    }

    get picturePath(): string {
      return this.user.picturePath && this.user.picturePath !== '' ? this.user.picturePath : 'uploads/unknown-user.jpg';
    }

    deleteUser(){
      const snackBarRef = this.snackBar.open(`WARNING Your profile will be deleted!`, 'Undo', { duration: 10000 });
      snackBarRef.afterDismissed().subscribe(res => {
        if(!res.dismissedByAction){
          this.userService.delete(this.user).subscribe(res =>{
            if(res){
              this.auth.logout();
              this.router.navigate(['/']);
            }
          });
        }
      });
    }

}
