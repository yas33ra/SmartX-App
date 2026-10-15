using System.Text.RegularExpressions;
using SmartX.API.Models;

namespace SmartX.API.Validation;

public static class SensorValidator
{
    // Six pairs of hex digits separated by ':' or '-', e.g. 00:1A:2B:3C:4D:5E
    private static readonly Regex MacPattern =
        new(@"^([0-9A-Fa-f]{2}[:-]){5}[0-9A-Fa-f]{2}$", RegexOptions.Compiled);

    public static List<string> Validate(Sensor sensor)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(sensor.Name))
            errors.Add("Sensor name is required.");

        if (string.IsNullOrWhiteSpace(sensor.MacAddress) || !MacPattern.IsMatch(sensor.MacAddress))
            errors.Add("MAC address must look like 00:1A:2B:3C:4D:5E.");

        if (string.IsNullOrWhiteSpace(sensor.Facility))
            errors.Add("Facility is required.");

        if (string.IsNullOrWhiteSpace(sensor.Zone))
            errors.Add("Zone is required.");

        if (string.IsNullOrWhiteSpace(sensor.SubZone))
            errors.Add("Sub-zone is required.");

        if (!Enum.IsDefined(typeof(SensorCategory), sensor.Category))
            errors.Add("Category is not valid.");

        // Only run the recursive check if the location fields are filled in
        if (errors.Count == 0 && !sensor.BuildDeploymentHierarchy().ValidateHierarchy())
            errors.Add("Deployment hierarchy is incomplete.");

        return errors;
    }
}