import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
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
  private auth = inject(AuthService);
  private captcha = inject(CaptchaService);

  loginForm!: FormGroup;
  passwordVisible = false;
  isSubmitting = false;
  isVerifyingCaptcha = false;
  captchaOk = false;
  errorMessage = '';
  successMessage = '';

  ngOnInit(): void {
    // Inicializar formulario vacío
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      remember: [false]
    });

    // Pre-llenar email si viene de registro (tiene prioridad)
    const emailParam = this.route.snapshot.queryParamMap.get('email');
    if (emailParam) {
      this.loginForm.patchValue({ email: emailParam });
      return; // No cargar el email recordado si viene uno del registro
    }

    // Cargar email recordado solo si existe
    const rememberedEmail = localStorage.getItem('rememberUser');
    if (rememberedEmail) {
      this.loginForm.patchValue({
        email: rememberedEmail,
        remember: true
      });
    }
  }

  get f() {
    return this.loginForm.controls;
  }

  // Mensajes de error específicos
  getErrorMessage(fieldName: string): string {
    const control = this.loginForm.get(fieldName);
    if (!control || !control.touched || !control.errors) return '';

    switch (fieldName) {
      case 'email':
        if (control.errors['required']) return 'El correo es obligatorio';
        if (control.errors['email']) return 'Formato de correo inválido';
        break;

      case 'password':
        if (control.errors['required']) return 'La contraseña es obligatoria';
        if (control.errors['minlength']) return 'Mínimo 8 caracteres';
        break;
    }

    return '';
  }

  togglePassword(): void {
    this.passwordVisible = !this.passwordVisible;
  }

  async onSubmit(): Promise<void> {
    this.errorMessage = '';
    this.successMessage = '';

    // Marcar campos como tocados para mostrar errores
    Object.values(this.loginForm.controls).forEach(c => c.markAsTouched());

    if (this.loginForm.invalid) {
      this.errorMessage = 'Por favor, corrige los campos marcados en rojo';
      return;
    }

    const { email, password, remember } = this.loginForm.value as {
      email: string;
      password: string;
      remember: boolean;
    };

    try {
      // 1) Verificar captcha
      this.isVerifyingCaptcha = true;
      this.captchaOk = false;

      const action = 'login';
      const token = await this.captcha.execute(action);
      const captchaRes = await this.captcha.validateOnServer(token, action).toPromise();

      this.captchaOk = !!captchaRes?.ok;
      this.isVerifyingCaptcha = false;

      if (!this.captchaOk) {
        this.errorMessage = 'Verificación de seguridad fallida. Por favor, inténtalo nuevamente';
        return;
      }

      // 2) Hacer login
      this.isSubmitting = true;

      this.auth.login(email.trim(), password).subscribe({
        next: (resp) => {
          this.isSubmitting = false;

          if (!resp.exito || !resp.usuario) {
            this.errorMessage = resp.mensaje ?? 'Correo o contraseña incorrectos';
            this.captchaOk = false; // Reset captcha en caso de error
            return;
          }

          // Guardar email si "Recordarme" está activado
          if (remember) {
            localStorage.setItem('rememberUser', email.trim());
          } else {
            localStorage.removeItem('rememberUser');
          }

          this.successMessage = '¡Inicio de sesión exitoso! Redirigiendo...';

          // Redirigir según el rol
          setTimeout(() => {
            const redirect = this.route.snapshot.queryParamMap.get('redirect');
            if (redirect) {
              this.router.navigateByUrl(redirect);
              return;
            }

            const rol = this.auth.role;
            const dest = rol === 'admin' ? '/admin' : rol === 'agent' ? '/agent' : '/client';
            this.router.navigate([dest]);
          }, 1000);
        },
        error: (err) => {
          this.isSubmitting = false;
          this.captchaOk = false; // Reset captcha en caso de error

          const apiError = err?.error;
          if (apiError?.mensaje) {
            this.errorMessage = apiError.mensaje;
          } else if (apiError?.errores && Array.isArray(apiError.errores)) {
            this.errorMessage = apiError.errores.join(' • ');
          } else if (err?.message) {
            this.errorMessage = err.message;
          } else {
            this.errorMessage = 'Error de conexión. Verifica tu red e inténtalo nuevamente';
          }
        }
      });

    } catch (e: any) {
      this.isVerifyingCaptcha = false;
      this.isSubmitting = false;
      this.captchaOk = false;
      this.errorMessage = e?.message ?? 'Error inesperado. Por favor, inténtalo nuevamente';
    }
  }
}
