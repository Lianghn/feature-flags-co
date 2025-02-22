import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { NzMessageService } from 'ng-zorro-antd/message';
import { LoginService } from 'src/app/services/login.service';
import { repeatPasswordValidator } from 'src/app/utils/validators';


@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.less', '../login.component.less']
})
export class RegisterComponent implements OnInit {

  registerForm!: FormGroup;

  isLoading: boolean = false;

  get password() {
    if (!this.registerForm || !this.registerForm.value) return '';
    return this.registerForm.value['password'];
  }

  get _password() {
    if (!this.registerForm || !this.registerForm.value) return '';
    return this.registerForm.value['_password'];
  }

  constructor(
    private loginService: LoginService,
    private router: Router,
    private message: NzMessageService
    ) { }

  ngOnInit(): void {
    this.initForm();
  }

  initForm() {
    this.registerForm = new FormGroup({
      email: new FormControl(null, [Validators.required, Validators.email]),
      password: new FormControl(null, [Validators.required, Validators.minLength(5)]),
      _password: new FormControl(null, [Validators.required]),
      phoneNumber: new FormControl(null, [Validators.required]),
    }, {
      validators: repeatPasswordValidator
    })
  }

  getPassword = (key: string = 'password') => this[key];

  doRegister() {
    if (this.registerForm.invalid) {
      for (const i in this.registerForm.controls) {
        this.registerForm.controls[i].markAsDirty();
        this.registerForm.controls[i].updateValueAndValidity();
      }
      return;
    }
    this.isLoading = true;
    const { _password, ...params } = this.registerForm.value
    this.loginService.register(params)
      .subscribe(
        res => {
          this.isLoading = false;
          this.message.success('注册成功！');
          this.router.navigateByUrl('/login');
        }
      );
  }

  changeRoute(url) {
    this.router.navigateByUrl(url);
  }

}
