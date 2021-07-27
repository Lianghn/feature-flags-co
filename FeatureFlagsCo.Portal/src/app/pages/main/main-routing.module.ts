import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ArchiveGuard } from './archive.guard';
import { MainComponent } from './main.component';
import { MainGuard } from './main.guard';

const routes: Routes = [
  {
    path: '',
    canActivate: [MainGuard],
    component: MainComponent,
    children: [
      {
        path: 'switch-manage',
        loadChildren: () => import("./switch-manage/switch-manage.module").then(m => m.SwitchManageModule)
      },
      {
        path: 'switch-user',
        loadChildren: () => import("./switch-user/switch-user.module").then(m => m.SwitchUserModule),
        data: {
          breadcrumb: '开关用户管理'
        },
      },
      {
        path: 'switch-archive',
        canActivate: [ArchiveGuard],
        loadChildren: () => import("./switch-archive/switch-archive.module").then(m => m.SwitchArchiveModule),
        data: {
          breadcrumb: '开关存档'
        },
      },
      {
        path: 'account-settings',
        loadChildren: () => import("./account-settings/account-settings.module").then(m => m.AccountSettingsModule),
        data: {
          breadcrumb: '账户管理'
        },
      },
      {
        path: '',
        redirectTo: '/main/switch-manage/index',
        pathMatch: 'full'
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class MainRoutingModule { }
