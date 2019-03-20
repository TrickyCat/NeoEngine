namespace TrickyCat.Text.TemplateEngines.NeoEngine.Tests.TemplateEngineTests

open TrickyCat.Text.TemplateEngines.NeoEngine.Services
open TrickyCat.Text.TemplateEngines.NeoEngine.ExecutionResults.Errors

module ``Template Engine Tests Common`` = 
    let private templateSvc = TemplateService() :> ITemplateService

    let renderOk : string -> Result<string, EngineError> = Ok

    let renderError : EngineError -> Result<string, EngineError> = Error

    let renderTemplate globals includes context template =
            templateSvc.RenderTemplateString globals includes template context
