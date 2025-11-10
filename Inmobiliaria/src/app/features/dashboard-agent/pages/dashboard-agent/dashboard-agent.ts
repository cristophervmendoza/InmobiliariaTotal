import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dashboard-agent',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard-agent.html',
  styleUrls: ['./dashboard-agent.css']
})
export class DashboardAgent implements OnInit {
  metrics = [
    { icon: '🏠', label: 'Propiedades Asignadas', value: 12, color: 'bg-blue' },
    { icon: '💰', label: 'Propiedades Vendidas', value: 5, color: 'bg-green' },
    { icon: '📅', label: 'Citas Pendientes', value: 3, color: 'bg-yellow' },
    { icon: '💬', label: 'Mensajes Nuevos', value: 4, color: 'bg-red' }
  ];

  recentActivity: string[] = [];
  upcomingMeetings: string[] = [];
  unreadMessages = 4;

  ngOnInit() {
    console.log('✅ Dashboard del agente cargado correctamente');
  }
}
