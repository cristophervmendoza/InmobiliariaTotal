import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { LoginService } from '../../../../core/services/login.service';
import { CaptchaService } from '../../../../core/services/captcha.service';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.html',
  styleUrls: ['./login.css']
})
export class Login implements OnInit {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private auth = inject(LoginService);
  private captcha = inject(CaptchaService);

  loginForm!: FormGroup;
  passwordVisible = false;
  isSubmitting = false;
  isVerifyingCaptcha = false;
  captchaOk = false;
  errorMessage = '';

  ngOnInit(): void {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      remember: [false]
    });
  }

  get f() { return this.loginForm.controls; }
  togglePassword(): void { this.passwordVisible = !this.passwordVisible; }

  async onSubmit(): Promise<void> {
    this.errorMessage = '';
    Object.values(this.loginForm.controls).forEach(c => c.markAsTouched());
    if (this.loginForm.invalid) {
      this.errorMessage = 'Por favor completa todos los campos correctamente';
      return;
    }

    const { email, password, remember } = this.loginForm.value as { email: string; password: string; remember: boolean; };

    try {
      // 1) Verificar captcha
      this.isVerifyingCaptcha = true;
      const action = 'login';
      const token = await this.captcha.execute(action);
      const captchaRes = await this.captcha.validateOnServer(token, action).toPromise();
      this.captchaOk = !!captchaRes?.ok;
      this.isVerifyingCaptcha = false;

      if (!this.captchaOk) {
        this.errorMessage = 'Captcha inválido, por favor inténtalo otra vez.';
        return;
      }

      // 2) Hacer login
      this.isSubmitting = true;
      this.auth.login({ email, password }).subscribe({
        next: (resp) => {
          if (!resp.exito || !resp.usuario) { this.errorMessage = resp.mensaje ?? 'Email o contraseña incorrectos'; return; }
          if (remember) localStorage.setItem('rememberUser', email);

          setTimeout(() => {
            const redirect = this.route.snapshot.queryParamMap.get('redirect');
            if (redirect) { this.router.navigateByUrl(redirect); return; }

            const rol = (resp.usuario!.rol as any)?.toLowerCase?.() ?? resp.usuario!.rol;
            const dest = rol === 'admin' ? '/admin' : rol === 'agent' ? '/agent' : '/client';
            this.router.navigate([dest]);
          }, 0);
        },
        error: (err) => {
          this.errorMessage = err?.error?.mensaje ?? 'Error de red';
        },
        complete: () => this.isSubmitting = false
      });
    } catch (e: any) {
      this.isVerifyingCaptcha = false;
      this.isSubmitting = false;
      this.errorMessage = e?.message ?? 'Error inesperado';
    }
  }
}
