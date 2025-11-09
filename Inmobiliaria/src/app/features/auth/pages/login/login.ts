import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.html',
  styleUrls: ['./login.css']
})
export class Login implements OnInit {
  loginForm!: FormGroup;
  passwordVisible = false;
  isSubmitting = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private auth: AuthService
  ) { }

  ngOnInit(): void {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
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

    this.isSubmitting = true;
    const { email, password, remember } = this.loginForm.value;

    try {
      await new Promise(r => setTimeout(r, 300));
      const session = this.auth.login(email, password);
      if (!session) { this.errorMessage = 'Email o contraseña incorrectos'; return; }

      if (remember) localStorage.setItem('rememberUser', email);

      const redirect = this.route.snapshot.queryParamMap.get('redirect');
      if (redirect) { this.router.navigateByUrl(redirect); return; }

      const dest = session.role === 'admin' ? '/admin'

        : session.role === 'agent' ? '/agent'
          : '/client';
      this.router.navigate([dest]);
    } finally {
      this.isSubmitting = false;
    }
  }
}
