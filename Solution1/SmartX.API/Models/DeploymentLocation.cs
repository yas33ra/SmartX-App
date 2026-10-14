//The purpose of this class is to validate that a node is correctly configured in the right zone/location
namespace SmartX.API.Models
{
    public class DeploymentLocation
    {
        // Facility -> Zone -> SubZone is 3 levels; 5 leaves headroom but stops runaway chains
        private const int MaxDepth = 5;

        public string Name { get; set; } = string.Empty;
        public bool IsTopLevel { get; set; }
        public DeploymentLocation? Parent { get; set; }

        // callers don't need to know about depth or visited nodes
        public bool ValidateHierarchy()
        {
            return ValidateHierarchy(0, new HashSet<DeploymentLocation>());
        }

        private bool ValidateHierarchy(int depth, HashSet<DeploymentLocation> visited)
        {
            // Guard 1: chain is too long
            if (depth > MaxDepth)
            {
                return false;
            }

            // Guard 2: we've been to this node before, so the chain loops
            if (!visited.Add(this))
            {
                return false;
            }

            // Base case: reached the top of the hierarchy
            if (IsTopLevel)
            {
                return true;
            }

            if (Parent == null || string.IsNullOrWhiteSpace(Parent.Name))
            {
                return false;
            }

            // same check on the parent, one level deeper
            return Parent.ValidateHierarchy(depth + 1, visited);
        }
    }
}