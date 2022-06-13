import { Component, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FormGroup } from '@angular/forms';
import { FormControl } from '@angular/forms';
import { FormBuilder } from '@angular/forms';
import { Validators } from '@angular/forms';
import * as _ from 'lodash-es';

@Component({
  selector: 'app-add-board',
  templateUrl: './add-board.component.html',
  styleUrls: ['./add-board.component.css']
})
export class AddBoardComponent implements OnInit {
  public frm: FormGroup;
  public ctlName: FormControl;

  constructor(public dialogRef: MatDialogRef<AddBoardComponent>, private fb: FormBuilder) { }

  ngOnInit(): void {
    this.ctlName = this.fb.control('', [Validators.required, Validators.minLength(3), Validators.maxLength(25)]);
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
