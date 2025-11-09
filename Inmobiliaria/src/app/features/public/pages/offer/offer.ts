import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators, FormGroup, FormControl } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

type OfferForm = {
  propertyType: FormControl<string>;
  fullAddress: FormControl<string>;
  district: FormControl<string>;
  description: FormControl<string>;
  image: FormControl<File | null>;
};

@Component({
  selector: 'app-offer',
  standalone: false,
  templateUrl: './offer.html',
  styleUrls: ['./offer.css'],
})
export class Offer implements OnInit {
  isSubmitting = false;
  submitMessage = '';
  imagePreview: string | null = null;

  form!: FormGroup<OfferForm>;

  get f(): OfferForm {
    return this.form.controls;
  }

  constructor(private fb: FormBuilder, private http: HttpClient) { }

  ngOnInit(): void {
    this.form = this.fb.group<OfferForm>({
      propertyType: this.fb.control('', { nonNullable: true, validators: [Validators.required] }),
      fullAddress: this.fb.control('', { nonNullable: true, validators: [Validators.required] }),
      district: this.fb.control('', { nonNullable: true, validators: [Validators.required] }),
      description: this.fb.control('', { nonNullable: true, validators: [Validators.required] }),
      image: this.fb.control<File | null>(null, { validators: [Validators.required] }),
    });
  }

  onFileChange(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    if (!['image/jpeg', 'image/png'].includes(file.type)) {
      this.form.get('image')?.setErrors({ type: true });
      this.imagePreview = null;
      return;
    }

    if (file.size > 5 * 1024 * 1024) {
      this.form.get('image')?.setErrors({ size: true });
      this.imagePreview = null;
      return;
    }

    this.form.patchValue({ image: file });
    this.form.get('image')?.updateValueAndValidity();

    const reader = new FileReader();
    reader.onload = () => (this.imagePreview = reader.result as string);
    reader.readAsDataURL(file);
  }

  removeImage() {
    this.form.patchValue({ image: null });
    this.form.get('image')?.updateValueAndValidity();
    this.imagePreview = null;
  }

  async onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;

    // Usa getRawValue para valores fuertemente tipados
    const v = this.form.getRawValue();

    const fd = new FormData();
    fd.append('propertyType', v.propertyType);
    fd.append('fullAddress', v.fullAddress);
    fd.append('district', v.district);
    fd.append('description', v.description);
    if (v.image) {
      fd.append('image', v.image);
    }

    try {
      // await this.http.post('https://tuapi.com/api/propiedades', fd).toPromise();
      await new Promise((r) => setTimeout(r, 1500));
      this.submitMessage = 'Propiedad enviada exitosamente';
      this.form.reset({
        propertyType: '',
        fullAddress: '',
        district: '',
        description: '',
        image: null
      });
      this.imagePreview = null;
      setTimeout(() => (this.submitMessage = ''), 3000);
    } finally {
      this.isSubmitting = false;
    }
  }
}
