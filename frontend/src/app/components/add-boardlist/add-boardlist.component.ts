import { Component } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { FormGroup } from '@angular/forms';
import { FormControl } from '@angular/forms';
import { FormBuilder } from '@angular/forms';
import { Validators } from '@angular/forms';
import * as _ from 'lodash-es';

@Component({
  selector: 'app-add-boardlist',
  templateUrl: './add-boardlist.component.html',
  styleUrls: ['./add-boardlist.component.css']
})
export class AddBoardListComponent {
  public frm: FormGroup;
  public ctlName: FormControl;
  public ConnectedUserid = +sessionStorage.getItem('ConnectedUserId');

  constructor(public dialogRef: MatDialogRef<AddBoardListComponent>, private fb: FormBuilder) {

    this.ctlName = this.fb.control('', [Validators.required, Validators.minLength(3), Validators.maxLength(15)]);

    this.frm = this.fb.group({
      name: this.ctlName
    });
  }

  cancel() {
    this.dialogRef.close();
  }

  close() {
    this.dialogRef.close(this.frm.value);
  }
}
