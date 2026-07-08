using Data.Repository;
using Data.Repository.Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using Microsoft.EntityFrameworkCore;
using Lockium.Data.LockiumDb.Entities.Devices;
using Lockium.Models.Dtos.Devices;
using Lockium.Models.Queries.Devices.Devices;
using Lockium.Mappings.Devices;
using Lockium.Data.LockiumDb.DatabaseContext;

namespace Lockium.Client.Api.Controllers.Devices
{
    /// <summary>
    /// Устройства (постаматы) — чтение каталога и состояния подключения.
    /// </summary>
    /// <remarks>
    /// Только операции чтения: поиск и получение по id. Список ячеек устройства подгружается вместе с устройством.
    /// Создание, изменение и удаление устройств через Client API не поддерживаются.
    /// </remarks>
    [Route("/api/v1/devices")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Client,SuperAdministrator,Administrator")]
    public partial class DevicesController : RestControllerBase2<Device, long, DeviceDto, DeviceQuery, DeviceMap>
    {
        public DevicesController(ILogger<RestServiceBase<Device, long>> logger,
            IDapperDbContext restDapperDb,
            LockiumDbContext restDb,
            DeviceMap deviceMap)
            : base(logger,
                restDapperDb,
                restDb,
                "Devices",
                deviceMap)
        {
        }

        /// <summary>
        /// Поиск устройств по фильтру.
        /// </summary>
        /// <remarks>
        /// Постраничный список постаматов. Для каждого устройства в ответе заполняется коллекция <c>Channels</c> (все ячейки этого устройства).
        /// Статус подключения: `1` — выключен, `2` — включён, `3` — ошибка.
        /// </remarks>
        /// <param name="query">Параметры поиска (<see cref="DeviceQuery"/>).</param>
        /// <response code="200">Страница устройств с ячейками.</response>
        /// <response code="400">Ошибка валидации запроса.</response>
        /// <response code="401">Не передан или недействителен JWT.</response>
        /// <response code="403">Недостаточно прав.</response>
        [Route("/api/v1/devices/search")]
        [HttpPost]
        [ProducesResponseType(typeof(PagedList<DeviceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<PagedList<DeviceDto>> SearchAsync([FromBody] DeviceQuery query)
        {
            return await SearchUsingEfAsync(query, null, apply: LoadChannelsAsync);
        }

        /// <summary>
        /// Получить устройство по идентификатору.
        /// </summary>
        /// <remarks>
        /// Возвращает постамат и полный список его ячеек (номер, состояние ячейки и замка, габариты).
        /// </remarks>
        /// <param name="key">Идентификатор устройства.</param>
        /// <response code="200">Устройство с ячейками.</response>
        /// <response code="400">Устройство не найдено.</response>
        /// <response code="401">Не передан или недействителен JWT.</response>
        /// <response code="403">Недостаточно прав.</response>
        [Route("/api/v1/devices/{key}")]
        [HttpGet]
        [ProducesResponseType(typeof(DeviceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Produces(MediaTypeNames.Application.Json)]
        [Consumes(MediaTypeNames.Application.Json)]
        public override async Task<DeviceDto> FindAsync([FromRoute] long key)
        {
            return await FindUsingEfAsync(key, null, apply: LoadChannelsAsync);
        }

        private async Task LoadChannelsAsync(List<Device> devices)
        {
            if (devices.Count == 0)
                return;

            var deviceIds = devices.Select(d => d.Id).ToList();
            var channels = await restDb.Set<Channel>()
                .AsNoTracking()
                .Where(c => c.DeviceId != null && deviceIds.Contains(c.DeviceId.Value))
                .ToListAsync();

            var channelsByDeviceId = channels
                .GroupBy(c => c.DeviceId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var device in devices)
            {
                device.Channels = channelsByDeviceId.TryGetValue(device.Id, out var list)
                    ? list
                    : [];
            }
        }

    }
}
