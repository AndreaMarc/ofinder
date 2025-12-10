namespace MIT.Fwk.Core.Models
{
    /// <summary>
    /// Rappresenta il valore dell'header Authorization per autenticazione JWT.
    /// Estrae e valida il token JWT dall'header "Authorization: Bearer {token}".
    /// </summary>
    public class JwtAuthenticationHeaderValue
    {
        private readonly string _authenticationHeaderValue;
        private string _token;

        /// <summary>
        /// Inizializza una nuova istanza di <see cref="JwtAuthenticationHeaderValue"/>.
        /// </summary>
        /// <param name="authenticationHeaderValue">Il valore dell'header Authorization (es. "Bearer eyJhbGc...").</param>
        public JwtAuthenticationHeaderValue(string authenticationHeaderValue)
        {
            if (!string.IsNullOrWhiteSpace(authenticationHeaderValue))
            {
                _authenticationHeaderValue = authenticationHeaderValue;
                if (TryDecodeHeaderValue())
                {
                    ReadAuthenticationHeaderValue();
                    IsValid = true;
                }
            }
        }

        /// <summary>
        /// Indica se l'header Authorization è valido e contiene un token JWT.
        /// </summary>
        public bool IsValid { get; private set; }

        /// <summary>
        /// Il token JWT estratto dall'header Authorization.
        /// </summary>
        public string JwtToken { get; set; }

        /// <summary>
        /// Tenta di decodificare l'header Authorization estraendo il token JWT.
        /// </summary>
        private bool TryDecodeHeaderValue()
        {
            const int headerSchemeLength = 7; // "Bearer ".Length;
            if (_authenticationHeaderValue.Length <= headerSchemeLength)
            {
                return false;
            }
            _token = _authenticationHeaderValue.Substring(headerSchemeLength);
            return true;
        }

        /// <summary>
        /// Legge il valore del token JWT dall'header.
        /// </summary>
        private void ReadAuthenticationHeaderValue()
        {
            JwtToken = _token;
        }
    }
}
