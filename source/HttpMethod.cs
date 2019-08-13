namespace Rest.API.Translator
{

    /// <summary>
    /// HttpClient Operation Type
    /// </summary>
    public enum MethodType
    {
        /// <summary>
        /// GET WITH PARAMETERS(OPTIONAL)
        /// </summary>
        GET,
        /// <summary>
        /// POST WITH PARAMETERS 
        /// </summary>
        POST,
        /// <summary>
        /// POST AN OBJECT as Json 
        /// </summary>
        JSONPOST
    }
}
