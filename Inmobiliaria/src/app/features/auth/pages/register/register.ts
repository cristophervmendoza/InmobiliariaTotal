import { Component, inject } from '@angular/core';
import { FormBuilder, Validators, FormGroup, ValidatorFn, AbstractControl } from '@angular/forms';
import { Router } from '@angular/router';
import { RegisterService, RegisterServerDto } from '../../../../core/services/register.service';

function sameAs(controlName: string, matchName: string): ValidatorFn {
  return (group: AbstractControl) => {
    const a = group.get(controlName)?.value;
    const b = group.get(matchName)?.value;
    return a === b ? null : { notMatch: true };
  };
}

@Component({
  selector: 'app-register',
  standalone: false,
  templateUrl: './register.html',
  styleUrls: ['./register.css'],
})
export class Register {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private registerSvc = inject(RegisterService);

  regPwdVisible = false;
  regPwd2Visible = false;
  isSubmitting = false;
  errorMessage = '';
  successMessage = '';

  form: FormGroup = this.fb.group({
    nombre: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
    dni: ['', [Validators.required, Validators.pattern(/^[0-9]{8}$/)]],
    telefono: ['', [Validators.required, Validators.pattern(/^9\d{8}$/)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\w\s]).{8,}$/)]],
    confirm: ['', [Validators.required]],
    terminos: [false, [Validators.requiredTrue]],
  }, { validators: sameAs('password', 'confirm') });

  get f() { return this.form.controls; }

  // Mensajes de error específicos para cada campo
  getErrorMessage(fieldName: string): string {
    const control = this.form.get(fieldName);
    if (!control || !control.touched || !control.errors) return '';

    switch (fieldName) {
      case 'nombre':
        if (control.errors['required']) return 'El nombre es obligatorio';
        if (control.errors['minlength']) return 'Mínimo 3 caracteres';
        if (control.errors['maxlength']) return 'Máximo 100 caracteres';
        break;

      case 'dni':
        if (control.errors['required']) return 'El DNI es obligatorio';
        if (control.errors['pattern']) return 'Debe tener exactamente 8 dígitos';
        break;

      case 'telefono':
        if (control.errors['required']) return 'El teléfono es obligatorio';
        if (control.errors['pattern']) return 'Debe empezar con 9 y tener 9 dígitos';
        break;

      case 'email':
        if (control.errors['required']) return 'El correo es obligatorio';
        if (control.errors['email']) return 'Formato de correo inválido';
        break;

      case 'password':
        if (control.errors['required']) return 'La contraseña es obligatoria';
        if (control.errors['pattern']) return 'Mínimo 8 caracteres: mayúscula, minúscula, número y símbolo';
        break;

      case 'confirm':
        if (control.errors['required']) return 'Debes confirmar la contraseña';
        if (this.form.errors?.['notMatch']) return 'Las contraseñas no coinciden';
        break;
    }

    return '';
  }

  private normalizePhone(nineDigits: string): string {
    return nineDigits.replace(/\D/g, '').slice(0, 9);
  }

  async onSubmit() {
    this.errorMessage = '';
    this.successMessage = '';

    // Marcar todos los campos como tocados para mostrar errores
    Object.values(this.form.controls).forEach(c => c.markAsTouched());

    if (this.form.invalid) {
      // Encontrar el primer campo inválido
      const firstInvalid = Object.keys(this.form.controls).find(
        key => this.form.get(key)?.invalid
      );

      if (firstInvalid === 'terminos') {
        this.errorMessage = 'Debes aceptar los términos y condiciones';
      } else {
        this.errorMessage = 'Por favor, corrige los campos marcados en rojo';
      }
      return;
    }

    this.isSubmitting = true;
    const { nombre, dni, telefono, email, password } = this.form.value as {
      nombre: string; dni: string; telefono: string; email: string; password: string;
    };

    const dto: RegisterServerDto = {
      Nombre: nombre.trim(),
      Dni: dni.trim(),
      Telefono: this.normalizePhone(telefono.trim()),
      Email: email.trim().toLowerCase(),
      Password: password,
      IdEstadoUsuario: 1
    };

    this.registerSvc.create(dto).subscribe({
      next: (res) => {
        this.isSubmitting = false;
        if (!res.exito) {
          this.errorMessage = res.mensaje ?? 'No se pudo registrar la cuenta';
          return;
        }
        this.successMessage = '¡Cuenta creada exitosamente! Redirigiendo...';
        this.form.reset();
        setTimeout(() => {
          this.router.navigate(['/auth/login'], { queryParams: { email: dto.Email } });
        }, 1500);
      },
      error: (err) => {
        this.isSubmitting = false;
        const api = err?.error;
        if (api?.errores && Array.isArray(api.errores)) {
          this.errorMessage = api.errores.join(' • ');
        } else if (api?.mensaje) {
          this.errorMessage = api.mensaje;
        } else if (err?.message) {
          this.errorMessage = err.message;
        } else {
          this.errorMessage = 'Error de conexión. Verifica tu red e inténtalo nuevamente';
        }
      }
    });
  }
}
