using System;
using System.Collections.Generic;
using System.Linq;

public class TokenManager<TTokenType>
{
    public delegate int TokenComparer(ManagedToken a, ManagedToken b);

    #region Private Fields

    private readonly HashSet<ManagedToken> _tokens = new();

    private readonly List<ManagedToken> _sortedTokens = new();
    private bool _hasBeenEditedSinceLastSort = true;

    private readonly ManagedToken _indefiniteToken;

    #endregion

    #region Getters

    public IReadOnlyList<ManagedToken> Tokens
    {
        get
        {
            // If the tokens are not sorted, return the tokens
            if (!IsSorted || TokenComparerFunction == null)
                return _tokens.ToArray();

            // If the tokens have been edited since the last sort, sort the tokens again
            if (_hasBeenEditedSinceLastSort)
            {
                // Create a sorted list of the tokens
                _sortedTokens.Clear();

                // Add the tokens to the sorted tokens
                _sortedTokens.AddRange(_tokens);

                // Sort the tokens based on the comparer
                _sortedTokens.Sort((a, b) => TokenComparerFunction(a, b));
            }

            return _sortedTokens;
        }
    }

    public bool IsSorted { get; private set; }

    public TokenComparer TokenComparerFunction { get; private set; }

    #endregion

    public TokenManager(bool isSorted, TokenComparer tokenComparerFunction, TTokenType indefiniteDefaultValue)
    {
        IsSorted = isSorted;
        TokenComparerFunction = tokenComparerFunction;

        // Add the indefinite default value
        if (indefiniteDefaultValue != null)
            _indefiniteToken = AddToken(indefiniteDefaultValue, 0, true);
    }

    public void Update(float deltaTime)
    {
        // Create a copy of the tokens to prevent concurrent modification
        var currentTokens = new List<ManagedToken>(_tokens);

        // Update the tokens
        foreach (var token in currentTokens)
        {
            // Update the token's timer
            token.timer.Update(deltaTime);

            // If the token is infinite, continue
            if (token.isInfinite)
                continue;

            // If the token is complete, remove it
            if (token.timer.Percentage >= 1)
                RemoveToken(token);
        }
    }

    public ManagedToken AddToken(TTokenType tokenValue, float duration, bool isInfinite = false)
    {
        // Create a token
        var token = new ManagedToken(tokenValue, duration, isInfinite);

        // Add the token to the tokens
        _tokens.Add(token);

        // Set the has been edited since last sort to true
        _hasBeenEditedSinceLastSort = true;

        return token;
    }

    public void RemoveToken(ManagedToken token)
    {
        // If the token is the indefinite token, return
        if (token == _indefiniteToken)
            return;

        // Remove the token from the tokens
        _tokens.Remove(token);

        // Set the has been edited since last sort to true
        _hasBeenEditedSinceLastSort = true;
    }

    public bool HasToken(ManagedToken token)
    {
        return _tokens.Contains(token);
    }

    public void Clear()
    {
        // Clear the tokens
        _tokens.Clear();

        // Set the has been edited since last sort to true
        _hasBeenEditedSinceLastSort = true;

        // Add the indefinite token back
        if (_indefiniteToken != null)
            _tokens.Add(_indefiniteToken);
    }

    public class ManagedToken
    {
        public TTokenType Value { get; set; }
        public readonly CountdownTimer timer;
        public readonly bool isInfinite;

        public ManagedToken(TTokenType value, float duration, bool isInfinite)
        {
            this.Value = value;
            this.timer = new CountdownTimer(duration, isActive: true);
            this.isInfinite = isInfinite;
        }
    }
}