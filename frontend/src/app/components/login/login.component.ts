import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators, FormControl } from '@angular/forms';

import { AuthenticationService } from '../../services/authentication.service';

@Component({
    templateUrl: 'login.component.html',
    styleUrls: ['login.component.css']
})
export class LoginComponent implements OnInit {
    loginForm: FormGroup;
    loading = false;    // utilisé en HTML pour désactiver le bouton pendant la requête de login
    submitted = false;  // retient si le formulaire a été soumis ; utilisé pour n'afficher les
    // erreurs que dans ce cas-là (voir template)
    returnUrl: string;
    ctlPseudo: FormControl;
    ctlPassword: FormControl;

    constructor(
        private formBuilder: FormBuilder,
        private route: ActivatedRoute,
        private router: Router,
        private auth: AuthenticationService
    ) {
        // redirect to home if already logged in
        if (this.auth.currentUser) {
            this.router.navigate(['/all-boards']);
        }
    }

    ngOnInit() {
        /**
         * Ici on construit le formulaire réactif. On crée un 'group' dans lequel on place deux
         * 'controls'. Remarquez que la méthode qui crée les controls prend comme paramêtre une
         * valeur initiale et un tableau de validateurs. Les validateurs vont automatiquement
         * vérifier les valeurs encodées par l'utilisateur et reçues dans les FormControls grâce
         * au binding, en leur appliquant tous les validateurs enregistrés. Chaque validateur
         * qui identifie une valeur non valide va enregistrer une erreur dans la propriété
         * 'errors' du FormControl. Ces erreurs sont accessibles par le template grâce au binding.
         */
        this.ctlPseudo = this.formBuilder.control('', Validators.required);
        this.ctlPassword = this.formBuilder.control('', Validators.required);
        this.loginForm = this.formBuilder.group({
            pseudo: this.ctlPseudo,
            password: this.ctlPassword
        });

        // get return url from route parameters or default to '/'
        this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
    }

    // On définit ici un getter qui permet de simplifier les accès aux champs du formulaire dans le HTML
    get f() { return this.loginForm.controls; }

    /**
     * Cette méthode est bindée sur l'événement onsubmit du formulaire. On va y faire le
     * login en faisant appel à AuthenticationService.
     */
    onSubmit() {
        this.submitted = true;

        // on s'arrête si le formulaire n'est pas valide
        if (this.loginForm.invalid) { return; }

        this.loading = true;
        this.auth.login(this.f.pseudo.value, this.f.password.value)
            .subscribe(
                // si login est ok, on navigue vers la page demandée
                data => {
                    this.router.navigate(['/all-boards']);
                },
                // en cas d'erreurs, on reste sur la page et on les affiche
                error => {
                    const errors = error.error.errors;
                    for (const field in errors) {
                        this.loginForm.get(field.toLowerCase()).setErrors({ custom: errors[field] });
                    }
                    this.loading = false;
                });
    }
}
