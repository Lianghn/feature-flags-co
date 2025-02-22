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
        loadChildren: () => import("./switch-manage/switch-manage.module").then(m => m.SwitchManageModule),
        data: {
          breadcrumb: '开关管理'
        },
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
        path: 'data-sync',
        loadChildren: () => import("./data-sync/data-sync.module").then(m => m.DataSyncModule),
        data: {
          breadcrumb: '数据同步'
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
        redirectTo: '/switch-manage',
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
