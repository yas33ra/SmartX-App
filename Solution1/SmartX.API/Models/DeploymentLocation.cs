//The purpose of this class is to validate that a node is correctly configured in the right zone/location
namespace SmartX.API.Models
{
    public class DeploymentLocation
    {
        public string Name { get; set; } = string.Empty;
        public bool IsTopLevel { get; set; }
        public DeploymentLocation? Parent { get; set; }

        public bool ValidateHierarchy()
        {
            if (IsTopLevel)
            {
                return true;
            }

            if (Parent == null || string.IsNullOrWhiteSpace(Parent.Name))
            {
                return false;
            }

            return Parent.ValidateHierarchy();
        }
    }
}
