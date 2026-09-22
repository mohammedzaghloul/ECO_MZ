using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.DAL.Sharing
{
    public static class EmailStringBody
    {
        public static string Send( string email,  string token, string component,  string message,   string frontendUrl)
        {
            string encodedToken = Uri.EscapeDataString(token);
            string encodedEmail = Uri.EscapeDataString(email);
            string encodedComponent = Uri.EscapeDataString(component);
            string accountUrl = $"{frontendUrl.TrimEnd('/')}/account/{encodedComponent}?email={encodedEmail}&code={encodedToken}";
            return $@"
`                <html>
                   <head></head>
                    <body>
                    <h1>{message}</h1>
                    <a href=""{accountUrl}""> {message}</a>
                   </body>
                </html>`";
        }
    }
}
