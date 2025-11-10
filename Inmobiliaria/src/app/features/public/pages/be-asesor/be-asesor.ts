import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators, FormGroup } from '@angular/forms';

@Component({
  selector: 'app-be-asesor',
  standalone: false,
  templateUrl: './be-asesor.html',
  styleUrls: ['./be-asesor.css'],
})
export class BeAsesor implements OnInit {
  isSubmitting = false;
  submitMessage = '';
  cvFileName = '';
  form!: FormGroup;

  constructor(private fb: FormBuilder) { }

  ngOnInit(): void {
    this.form = this.fb.group({
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      age: ['', [Validators.required, Validators.min(18), Validators.max(65)]],
      phone: ['', [Validators.required, Validators.minLength(9)]],
      email: ['', [Validators.required, Validators.email]],
      address: ['', [Validators.required]],
      realEstateExperience: ['', [Validators.required]],
      cv: [null, [Validators.required]],
      availability: ['', [Validators.required]]
    });
  }

  get f() { return this.form.controls; }
  onFileChange(ev: Event): void {
    const input = ev.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    if (file.type !== 'application/pdf') {
      this.form.get('cv')?.setErrors({ type: true });
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      this.form.get('cv')?.setErrors({ size: true });
      return;
    }
    this.form.patchValue({ cv: file });
    this.cvFileName = file.name;
    this.form.get('cv')?.updateValueAndValidity();
  }
  removeCv(): void {
    this.form.patchValue({ cv: null });
    this.cvFileName = '';
  }
  async onSubmit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.isSubmitting = true;
    try {
      const fd = new FormData();
      Object.entries(this.form.value).forEach(([k, v]) => {
        if (k === 'cv' && v instanceof File) fd.append('cv', v);
        else if (k !== 'cv') fd.append(k, String(v));
      });
      await new Promise(r => setTimeout(r, 1200));
      this.submitMessage = '¡Aplicación enviada exitosamente!';
      setTimeout(() => { this.form.reset(); this.cvFileName = ''; this.submitMessage = ''; }, 3000);
    } catch {
      this.submitMessage = 'Error al enviar la aplicación. Intenta de nuevo.';
    } finally {
      this.isSubmitting = false;
    }
  }
  onReset(): void {
    this.form.reset();
    this.cvFileName = '';
  }
}
