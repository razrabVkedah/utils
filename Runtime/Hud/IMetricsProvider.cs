namespace Rusleo.Utils.Runtime.Hud
{
    public interface IMetricsProvider
    {
        string Name { get; }
        bool Enabled { get; set; }

        // Вызывается раз в кадр (или реже, если throttle в сервисе)
        void Update(float dt);

        // Пишем короткую строку в общий буфер (без аллокаций)
        void Emit(IStringBuilderTarget sb);
    }
}