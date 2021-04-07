using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Model
{
    public class EntityChangedCommand
    {
        public string EntitetsId { get; internal set; }
        public EntityState State { get; internal set; }
        public string EntityType { get; internal set; }
    }

    public static class EntityChangedCommandEx
    {
        public static List<EntityChangedCommand> ToEntityChangedCommand(this IEnumerable<EntityEntry> entityEntries)
            => entityEntries.Select(_ => new EntityChangedCommand { 
                EntitetsId = (_.Entity as Entitet).Id,
                State = _.State,
                EntityType = _.Metadata.ClrType.Name
            }).ToList();
    }
}
