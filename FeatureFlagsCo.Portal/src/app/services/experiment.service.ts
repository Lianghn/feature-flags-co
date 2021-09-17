import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { environment } from 'src/environments/environment';
import { IProject, IProjectEnv } from '../config/types';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class ExperimentService {

  baseUrl: string = environment.url + '/api/experiments/CustomEvents/#envId';
  currentProjectEnvChanged$: Subject<void> = new Subject();

  constructor(
    private http: HttpClient
  ) {}

  // 获取 custom events 列表
  public getCustomEvents(envId: number): Observable<string[]> {
    const url = this.baseUrl.replace(/#envId/ig, `${envId}`);
    return this.http.get<string[]>(url);
  }
}
