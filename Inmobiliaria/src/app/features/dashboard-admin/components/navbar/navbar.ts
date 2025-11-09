import { Component, ElementRef, HostListener, ViewChild } from '@angular/core';

@Component({
  selector: 'app-navbar',
  standalone: false,
  templateUrl: './navbar.html',
  styleUrls: ['./navbar.css']
})
export class Navbar {
  showDropdown = false;
  userName = 'Sandra Suárez';
  userRole = 'Administrador';

  @ViewChild('userMenuRoot') userMenuRoot!: ElementRef<HTMLElement>;

  get initials(): string {
    return this.userName
      .split(' ')
      .filter(Boolean)
      .map(n => n[0]!)
      .join('')
      .toUpperCase();
  }

  toggleDropdown(ev?: MouseEvent): void {
    if (ev) ev.stopPropagation();
    this.showDropdown = !this.showDropdown;
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.showDropdown) return;
    const root = this.userMenuRoot?.nativeElement;
    const target = event.target as Node | null;
    if (!root || !target) return;
    if (!root.contains(target)) {
      this.showDropdown = false;
    }
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.showDropdown) this.showDropdown = false;
  }
}
