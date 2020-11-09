namespace Company.Function
open System
open System.IO
open Microsoft.AspNetCore.Mvc
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open System.Text.Json
open System.Text.Json.Serialization 
open Newtonsoft.Json

module HttpTrigger1 =

    type DTO = { Name: string; Description : string } with static member Empty = { Name = ""; Description = "" }

    // For convenience, it's better to have a central place for the literal.
    [<Literal>]
    let QueryName = "name"
    [<Literal>]
    let QueryDescription = "description"

    [<FunctionName("GetMyName")>]
    let run ([<HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)>]request: HttpRequest) (log: ILogger) =
        async {
            log.LogInformation("Starting")
            let! responseMessage =
                match request.Method with
                | "GET" ->
                    log.LogInformation("GET")
                    DTO.Empty
                    |> (fun d -> if request.Query.ContainsKey(QueryName) then log.LogInformation("Name"); { d with Name = request.Query.[QueryName].[0] } else d )
                    |> (fun d -> if request.Query.ContainsKey(QueryDescription) then log.LogInformation("Description");{ d with Description = request.Query.[QueryDescription].[0] } else d)
                    |> async.Return
                | "POST" ->
                    log.LogInformation("POST")
                    async {
                        use stream = new StreamReader(request.Body)
                        let! body = stream.ReadToEndAsync() |> Async.AwaitTask
                        let dto = JsonConvert.DeserializeObject<DTO>(body)
                        return dto
                    }
                | _ ->
                    log.LogInformation("Method is not supported")
                    { Name = ""; Description = "" } 
                    |> async.Return

            return OkObjectResult(responseMessage) :> IActionResult
        } |> Async.StartAsTask