using System.Net;
using System.Net.Mail;

namespace WA_ControlPresupuesto.Services
{
    public interface IServicioEmail
    {
        Task EnviarEmailCambioPassword(string receptor, string enlace);
    }

    public class ServicioEmail : IServicioEmail
    {
        private readonly IConfiguration configuration;

        public ServicioEmail(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task EnviarEmailCambioPassword(string receptor, string enlace)
        {
            var email = configuration.GetValue<string>("CONFIGURACIONES_EMAIL:EMAIL");
            var password = configuration.GetValue<string>("CONFIGURACIONES_EMAIL:PASSWORD");
            var host = configuration.GetValue<string>("CONFIGURACIONES_EMAIL:HOST");
            var puerto = configuration.GetValue<int>("CONFIGURACIONES_EMAIL:PUERTO");

            var cliente = new SmtpClient(host, puerto);
            cliente.EnableSsl = true;
            cliente.UseDefaultCredentials = false;

            cliente.Credentials = new NetworkCredential(email, password);
            var emisor = email;
            var subject = "¿Ha olvidado su contraseña";
            var contenidoHtml = $@"Saludos,
Este mensaje le llega porque usted ha solicitado un cambio de contraseña. Si esta solicitud no fue hecha por usted, puede ignorar este mensaje.
Para cambiar su contraseña, haga clic en el siguiente enlace: {enlace}

Atentamente,

Equipo Manejo Presupuesto";

            var mensaje = new MailMessage(emisor, receptor, subject, contenidoHtml);
            await cliente.SendMailAsync(mensaje);

        }
    }
}
