# 🎚️ Volume Controller - WPF + WebSocket

Uma aplicação moderna desenvolvida em **WPF** com **MahApps.Metro** para controlar o volume do sistema de forma visual e interativa, com suporte a **controle remoto via WebSocket**.

![Exemplo da Interface](images/image.png)

---

## 🔧 Funcionalidades

- Interface elegante com MahApps.Metro
- Controle visual do volume com animações
- Atualização em tempo real do volume via WebSocket
- Sincronização com o volume atual do sistema no início
- Ícone de bandeja com menu de contexto
- Indicador de status da conexão WebSocket

---

## 🚀 Tecnologias

- [.NET WPF (Windows Presentation Foundation)](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/)
- [MahApps.Metro](https://mahapps.com/)
- [NAudio](https://github.com/naudio/NAudio) – manipulação do volume do sistema
- [WebSocketSharp](https://github.com/sta/websocket-sharp) – comunicação em tempo real
- Ícone de bandeja usando `System.Windows.Forms.NotifyIcon`

---

## 📦 Instalação

1. **Clone o repositório**:

   ```bash
   git clone https://github.com/MaiconMJS/volume-dotnet-wpf.git
   cd volume-dotnet-wpf
   ```

2. **Abra no Visual Studio** e restaure os pacotes NuGet:

   - `MahApps.Metro`
   - `NAudio`
   - `WebSocketSharp`

3. **Compile e execute** o projeto.

> ⚠️ **Requisitos**: Windows com .NET Desktop Runtime instalado.

---

## 📡 WebSocket

- O aplicativo se conecta automaticamente ao servidor WebSocket definido:

  ```csharp
  new WebSocket("ws://localhost:3001")
  ```
