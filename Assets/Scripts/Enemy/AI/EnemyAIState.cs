namespace HorrorGame.Enemy
{
    /// <summary>
    /// Estados possíveis da IA do inimigo
    /// </summary>
    public enum EnemyAIState
    {
        Idle,           // Parado ou em animação idle
        Patrol,         // Patrulhando pontos predefinidos
        Investigate,    // Investigando última posição conhecida do player
        Chase,          // Perseguindo o player ativamente
        Attack,         // Atacando o player
        Stunned,        // Atordoado (após levar dano, etc)
        Retreat,        // Recuando (vida baixa, etc)
        Dead            // Morto
    }
}
