namespace Lockium.Workflows.Models
{
    /// <summary>
    /// Запрос на выполнение перехода состояния объекта workflow.
    /// </summary>
    public class WorkflowRunRequest
    {
        /// <summary>
        /// Идентификатор заказа (или другой сущности), для которой выполняется переход.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Текущее состояние до перехода (должно совпадать с фактическим состоянием в БД).
        /// </summary>
        public int PreviousState { get; set; }

        /// <summary>
        /// Целевое состояние после перехода.
        /// </summary>
        public int NextState { get; set; }
    }
}
