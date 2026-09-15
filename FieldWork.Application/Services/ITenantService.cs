using System;
using System.Collections.Generic;
using System.Text;

namespace FieldWork.Application.Services;

public interface ITenantService
{
    Task<bool> SetFaceVerificationRequiredAsync(
        bool required,
        CancellationToken cancellationToken = default);
}
