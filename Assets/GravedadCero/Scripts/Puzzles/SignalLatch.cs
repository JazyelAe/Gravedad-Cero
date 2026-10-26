namespace GravedadCero.Puzzles
{
    /// <summary>
    /// Señal sin cuerpo: un interruptor la activa y una ActivationGate la lee. Sirve para que
    /// una compuerta necesite varios interruptores (o un interruptor y una placa) a la vez.
    /// No hace nada al cambiar de estado; el indicador lo pone el propio interruptor.
    /// </summary>
    public class SignalLatch : Activatable
    {
        protected override void OnActivationChanged(bool activated, bool instant) { }
    }
}
