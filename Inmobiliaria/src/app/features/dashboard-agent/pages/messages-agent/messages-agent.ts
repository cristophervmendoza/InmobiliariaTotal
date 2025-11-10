import { Component, type OnInit } from "@angular/core"

interface Propiedad {
  id: number
  titulo: string
  imagen: string
}

interface Mensaje {
  id: number
  clienteNombre: string
  clienteTelefono: string
  clienteDni: string
  clienteEmail: string
  propiedad: Propiedad
  mensaje: string
  fecha: string
  leido: boolean //ESTO ES PARA QUE MARQUE EL AGENTE Y ASI VEA CUAL FALTA REVISAR
  respondido: boolean
}

@Component({
  selector: "app-messages-agent",
  templateUrl: "./messages-agent.html",
  styleUrls: ["./messages-agent.css"],
  standalone: false,
})
export class MessagesAgent implements OnInit {
  mensajes: Mensaje[] = []
  filteredMensajes: Mensaje[] = []
  loading = true
  searchText = ""
  filterType = "todos"
  selectedMensaje: Mensaje | null = null
  respuesta = ""
  showProfileModal = false

  constructor() { }

  ngOnInit(): void {
    this.loadMensajes()
  }

  loadMensajes(): void {
    try {
      // TODO: CONECTAR CON BACKEND
      // const response = await fetch('/api/agente/mensajes')
      // const data = await response.json()
      // this.mensajes = data

      const mockMensajes: Mensaje[] = [
        {
          id: 1,
          clienteNombre: "Juan Pérez",
          clienteTelefono: "987654321",
          clienteDni: "72345678",
          clienteEmail: "juan.perez@gmail.com",
          propiedad: {
            id: 1,
            titulo: "Casa moderna en Barranco",
            imagen: "https://via.placeholder.com/80x80",
          },
          mensaje:
            "Hola, estoy interesado en la propiedad 'Casa moderna en Barranco'. Me gustaría obtener más información sobre precio, disponibilidad y características.",
          fecha: "2025-11-05 10:30",
          leido: false,
          respondido: false,
        },
        {
          id: 2,
          clienteNombre: "María López",
          clienteTelefono: "998877665",
          clienteDni: "45678912",
          clienteEmail: "maria.lopez@gmail.com",
          propiedad: {
            id: 2,
            titulo: "Departamento céntrico",
            imagen: "https://via.placeholder.com/80x80",
          },
          mensaje: "Buenos días, quisiera agendar una visita para este departamento. ¿Cuándo estaría disponible?",
          fecha: "2025-11-05 09:15",
          leido: true,
          respondido: false,
        },
        {
          id: 3,
          clienteNombre: "Carlos Ruiz",
          clienteTelefono: "912345678",
          clienteDni: "12345678",
          clienteEmail: "carlos.ruiz@gmail.com",
          propiedad: {
            id: 3,
            titulo: "Terreno en Surco",
            imagen: "https://via.placeholder.com/80x80",
          },
          mensaje: "Me interesa el terreno. ¿Tiene servicios básicos?",
          fecha: "2025-11-04 16:45",
          leido: true,
          respondido: true,
        },
      ]

      this.mensajes = mockMensajes
      this.applyFilters()
    } catch (error) {
      console.error("Error:", error)
    } finally {
      this.loading = false
    }
  }

  applyFilters(): void {
    let filtered = [...this.mensajes]

    if (this.searchText) {
      filtered = filtered.filter(
        (m) =>
          m.clienteNombre.toLowerCase().includes(this.searchText.toLowerCase()) ||
          m.propiedad.titulo.toLowerCase().includes(this.searchText.toLowerCase()),
      )
    }

    if (this.filterType === "pendientes") {
      filtered = filtered.filter((m) => !m.respondido)
    } else if (this.filterType === "respondidos") {
      filtered = filtered.filter((m) => m.respondido)
    }

    this.filteredMensajes = filtered
  }

  onSearchChange(): void {
    this.applyFilters()
  }

  onFilterChange(tipo: string): void {
    this.filterType = tipo
    this.applyFilters()
  }

  handleSelectMensaje(mensaje: Mensaje): void {
    this.selectedMensaje = mensaje
    this.respuesta = ""

    if (!mensaje.leido) {
      // TODO: CONECTAR CON BACKEND
      // await fetch(`/api/mensajes/${mensaje.id}/marcar-leido`, { method: 'PUT' })

      mensaje.leido = true
      this.mensajes = [...this.mensajes]
    }
  }

  handleEnviarRespuesta(): void {
    if (!this.selectedMensaje || !this.respuesta.trim()) return

    try {
      // TODO: CONECTAR CON BACKEND
      // await fetch('/api/mensajes/responder', {
      //   method: 'POST',
      //   headers: { 'Content-Type': 'application/json' },
      //   body: JSON.stringify({
      //     idMensaje: this.selectedMensaje.id,
      //     respuesta: this.respuesta
      //   })
      // })

      alert("✅ Respuesta enviada correctamente")

      if (this.selectedMensaje) {
        this.selectedMensaje.respondido = true
        this.mensajes = [...this.mensajes]
      }

      this.respuesta = ""
    } catch (error) {
      console.error("Error:", error)
      alert("❌ Error al enviar respuesta")
    }
  }

  handleWhatsApp(): void {
    if (!this.selectedMensaje) return

    const phone = this.selectedMensaje.clienteTelefono.replace(/[^0-9]/g, "")
    const mensaje = encodeURIComponent(this.respuesta || "Hola, te contacto desde la inmobiliaria.")
    window.open(`https://wa.me/51${phone}?text=${mensaje}`, "_blank")
  }

  onProfileOpen(): void {
    this.showProfileModal = true
  }

  onProfileClose(): void {
    this.showProfileModal = false
  }

  handleCall(): void {
    if (this.selectedMensaje) {
      window.open(`tel:${this.selectedMensaje.clienteTelefono}`)
    }
  }

  handleEmail(): void {
    if (this.selectedMensaje) {
      window.open(`mailto:${this.selectedMensaje.clienteEmail}`)
    }
  }

  getPendientesCount(): number {
    return this.mensajes.filter((m) => !m.respondido).length
  }

  getTotalCount(): number {
    return this.mensajes.length
  }
}
