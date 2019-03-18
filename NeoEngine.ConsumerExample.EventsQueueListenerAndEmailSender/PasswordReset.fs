namespace TrickyCat.Text.TemplateEngines.NeoEngine.ConsumerExample.FSharp

open TrickyCat.Text.TemplateEngines.NeoEngine.Runners.Helpers
open System.Collections.Generic
open Common

module PasswordReset =

    let private template = "
<html>
<title>
    Dear user your password was reset.
</title>
<body>
<h1>Hello <%= name %>.</h1>
<p>Your password has been reset to <strong><%= newPassword %></strong></p>
</body>
</html>"

    let private templateRenderer: KeyValuePair<string, string> seq -> Result<string, _> =
        renderSvc.RenderTemplateString emptyGlobals emptyIncludes template

    let renderWithContext = templateRenderer

