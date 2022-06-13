import { AuthenticationService } from './../../services/authentication.service';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormControl, AsyncValidatorFn} from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { UserService } from '../../services/user.service';

@Component({
    selector: 'app-signup',
    templateUrl: './signup.component.html',
    styleUrls: ['./signup.component.css']
})
export class SignUpComponent implements OnInit {
    signupForm: FormGroup;
    ctlPseudo: FormControl;
    ctlBirthDate: FormControl;
    ctlEmail: FormControl;
    ctlFirstName: FormControl;
    ctlLastName: FormControl;
    ctlPassword: FormControl;
    ctlPasswordC: FormControl;

    returnUrl: string;
    loading = false;    // utilisé en HTML pour désactiver le bouton pendant la requête de login
    submitted = false;  // retient si le formulaire a été soumis ; utilisé pour n'afficher les

    constructor(
      private router: Router,
      private formBuilder: FormBuilder,
      private route: ActivatedRoute,
      private userService: UserService,
      private auth: AuthenticationService,
    ) {
      if (this.auth.currentUser) {
        this.router.navigate(['/boards']);
      }
    }


    ngOnInit(): void {
        this.ctlPseudo = this.formBuilder.control('', [Validators.required, Validators.minLength(3)
          , Validators.maxLength(10), this.forbiddenValue('abc'), Validators.pattern(/^[a-zA-Z]([a-zA-Z0-9_])+$/)], this.pseudoUsed());
        this.ctlFirstName = this.formBuilder.control('', [Validators.minLength(3), Validators.maxLength(50)]);
        this.ctlLastName = this.formBuilder.control('', [Validators.minLength(3), Validators.maxLength(50)]);
        this.ctlEmail = this.formBuilder.control('', [Validators.required, Validators.pattern(/^[^@\s]+@[^@\s]+\.[^@\s]+$/)],
        this.emailUsed());
        this.ctlBirthDate = this.formBuilder.control('');
        this.ctlPassword = this.formBuilder.control('', [Validators.required, Validators.minLength(3), Validators.maxLength(10)]);
        this.ctlPasswordC = this.formBuilder.control('', [Validators.required, Validators.minLength(3), Validators.maxLength(10)]);

        this.signupForm = this.formBuilder.group({
            pseudo: this.ctlPseudo,
            firstName: this.ctlFirstName,
            lastName: this.ctlLastName,
            email: this.ctlEmail,
            birthDate: this.ctlBirthDate,
            password: this.ctlPassword,
            passwordC: this.ctlPasswordC,
        },
            {
                validator: this.MustMatch('password', 'passwordC')
            }
        );
        this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
    }

    // On définit ici un getter qui permet de simplifier les accès aux champs du formulaire dans le HTML
    get f() { return this.signupForm.controls; }

    MustMatch(controlName: string, matchingControlName: string) {
        return (formGroup: FormGroup) => {
            const control = formGroup.controls[controlName];
            const matchingControl = formGroup.controls[matchingControlName];

            if (matchingControl.errors && !matchingControl.errors.mustMatch) {
                // return if another validator has already found an error on the matchingControl
                return;
            }

            // set error on matchingControl if validation fails
            if (control.value !== matchingControl.value) {
                matchingControl.setErrors({ mustMatch: true });
            } else {
                matchingControl.setErrors(null);
            }
        }
    }

    forbiddenValue(val: string): any {
      return (ctl: FormControl) => {
          if (ctl.value === val) {
              return { forbiddenValue: { currentValue: ctl.value, forbiddenValue: val } };
          }
          return null;
      };
    }

    // Validateur asynchrone qui vérifie si le pseudo n'est pas déjà utilisé par un autre membre
    pseudoUsed(): AsyncValidatorFn {
      let timeout: NodeJS.Timer;
      return (ctl: FormControl) => {
          clearTimeout(timeout);
          const pseudo = ctl.value;
          return new Promise(resolve => {
              timeout = setTimeout(() => {
                  if (ctl.pristine) {
                      resolve(null);
                  } else {
                      this.userService.getByPseudo(pseudo).subscribe(user => {
                          resolve(user ? { pseudoUsed: true } : null);
                      });
                  }
              }, 300);
          });
      };
    }

    emailUsed(): AsyncValidatorFn {
        let timeout: NodeJS.Timer;
        return (ctl: FormControl) => {
            clearTimeout(timeout);
            const email = ctl.value;
            return new Promise(resolve => {
                timeout = setTimeout(() => {
                    if (ctl.pristine) {

                    } else {
                        this.userService.getByEmail(email).subscribe(user => {
                            resolve(user ? { emailUsed: true } : null);
                        });
                    }
                }, 300);
            });
        };
    }

    /**
     * Cette méthode est bindée sur l'événement onsubmit du formulaire. On va y faire le
     * login en faisant appel à AuthenticationService.
     */
    onSubmit() {
      this.submitted = true;

      // on s'arrête si le formulaire n'est pas valide
      if (this.signupForm.invalid) { return; }

      this.loading = true;
      this.userService.add(this.signupForm.value)
          .subscribe(
              res => {
                  this.auth.login(this.f.pseudo.value, this.f.password.value).subscribe(
                      res => {
                          this.router.navigate([this.returnUrl]);
                      }
                  );
              },
              error => {
                  const errors = error.error.errors;
                  for (let field in errors) {
                      this.signupForm.get(field.toLowerCase()).setErrors({ custom: errors[field] })
                  }
                  this.loading = false;
              });
  }

}
