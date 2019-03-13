namespace TrickyCat.Text.TemplateEngines.NeoEngine.Tests.TemplateEngineTests

open TrickyCat.Text.TemplateEngines.NeoEngine.Services
open TrickyCat.Text.TemplateEngines.NeoEngine.Runners.RunnerErrors

module ``Template Engine Tests Common`` = 
    let private templateSvc = TemplateService() :> ITemplateService

    let renderOk : string -> Result<string, RunnerError> = Ok

    let renderError : RunnerError -> Result<string, RunnerError> = Error

    let renderTemplate globals includes context template =
            templateSvc.RenderTemplateString globals includes template context
