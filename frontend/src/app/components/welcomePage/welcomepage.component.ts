import { Component } from "@angular/core";
import { Router } from "@angular/router";
import { AuthenticationService } from "src/app/services/authentication.service";
import { GeneralHubService } from '../../services/general-hub.service';

@Component({
    selector: 'app-welcome-page',
    templateUrl: './welcomepage.component.html',
    styleUrls: ['./welcomepage.component.css']
})
export class WelcomePageComponent {

  constructor(private router: Router, private auth: AuthenticationService) {
        // redirect to home if already logged in
        if (this.auth.currentUser) {
            this.router.navigate(['/all-boards']);
        }
  }

    login() {
      this.router.navigate(['/login']);
    }

    signup() {
      this.router.navigate(['/signup']);
    }
}
