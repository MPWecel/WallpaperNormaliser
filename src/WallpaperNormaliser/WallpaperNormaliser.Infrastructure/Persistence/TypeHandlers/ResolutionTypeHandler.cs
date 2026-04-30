using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using WallpaperNormaliser.Core.Models.Common;

namespace WallpaperNormaliser.Infrastructure.Persistence.TypeHandlers;
public class ResolutionTypeHandler : SqlMapper.TypeHandler<Resolution>
{
    public override Resolution? Parse(object value)
    {
        if(value is null or DBNull)
            return null;

        string? valueString = value as string;

        if(String.IsNullOrEmpty(valueString))
            return null;

        Resolution? result = Resolution.FromString(valueString!);
        
        return result;
    }

    public override void SetValue(IDbDataParameter parameter, Resolution? value)
        => parameter.Value = ((object?)(value?.ToString()) ?? DBNull.Value);
}
