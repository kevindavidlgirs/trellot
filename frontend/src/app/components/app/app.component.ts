import { Component } from '@angular/core';
import { GeneralHubService } from 'src/app/services/general-hub.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent {
  title = 'app';

  constructor(private ghs: GeneralHubService){ this.ghs.connect(); }
}
