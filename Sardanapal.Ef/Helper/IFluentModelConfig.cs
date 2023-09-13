using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sardanapal.Ef.Helper
{
    public abstract class FluentModelConfig<T> where T : class
    {
        public abstract void OnModelBuild(EntityTypeBuilder<T> entity);
    }
}
