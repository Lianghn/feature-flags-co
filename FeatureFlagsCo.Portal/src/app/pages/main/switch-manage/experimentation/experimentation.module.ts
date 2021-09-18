import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ExperimentationRoutingModule } from './experimentation-routing.module';
import { ComponentsModule } from '../components/components.module';
import { ExperimentationComponent } from './experimentation.component';
import { NzEmptyModule } from 'ng-zorro-antd/empty';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzDatePickerModule } from 'ng-zorro-antd/date-picker';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { FormsModule } from '@angular/forms';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzCardModule } from 'ng-zorro-antd/card';
import { NzSpinModule } from 'ng-zorro-antd/spin';

@NgModule({
  declarations: [ExperimentationComponent],
  imports: [
    CommonModule,
    FormsModule,
    NzEmptyModule,
    NzSelectModule,
    NzDatePickerModule,
    NzTableModule,
    NzButtonModule,
    NzIconModule,
    NzCardModule,
    NzSpinModule,
    ComponentsModule,
    ExperimentationRoutingModule
  ]
})
export class ExperimentationModule { }
