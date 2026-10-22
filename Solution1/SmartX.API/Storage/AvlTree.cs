namespace SmartX.API.Storage;

/// <summary>
/// A self-balancing binary search tree (AVL tree).
/// Keeps keys sorted and stays balanced, so insert, lookup and
/// range queries are fast even with a lot of data.
/// </summary>
public class AvlTree<TKey, TValue> where TKey : IComparable<TKey>
{
    private class Node
    {
        public TKey Key;
        public TValue Value;
        public Node? Left;
        public Node? Right;
        public int Height = 1;   // a new node is a leaf, so height 1

        public Node(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }

    private Node? _root;

    public int Count { get; private set; }
    public int Height => HeightOf(_root);

    // ---------- helpers ----------

    private static int HeightOf(Node? node) => node?.Height ?? 0;

    private static void UpdateHeight(Node node) =>
        node.Height = 1 + Math.Max(HeightOf(node.Left), HeightOf(node.Right));

    // positive = left side is taller, negative = right side is taller
    private static int BalanceOf(Node node) =>
        HeightOf(node.Left) - HeightOf(node.Right);

    private static Node RotateRight(Node y)
    {
        var x = y.Left!;
        y.Left = x.Right;
        x.Right = y;
        UpdateHeight(y);
        UpdateHeight(x);
        return x;   // x is the new top of this part of the tree
    }

    private static Node RotateLeft(Node x)
    {
        var y = x.Right!;
        x.Right = y.Left;
        y.Left = x;
        UpdateHeight(x);
        UpdateHeight(y);
        return y;
    }

    private static Node Rebalance(Node node)
    {
        UpdateHeight(node);
        int balance = BalanceOf(node);

        if (balance > 1)                       // too heavy on the left
        {
            if (BalanceOf(node.Left!) < 0)     // left child leans right
                node.Left = RotateLeft(node.Left!);
            return RotateRight(node);
        }

        if (balance < -1)                      // too heavy on the right
        {
            if (BalanceOf(node.Right!) > 0)    // right child leans left
                node.Right = RotateRight(node.Right!);
            return RotateLeft(node);
        }

        return node;                           // already balanced
    }

    // ---------- insert ----------

    /// <summary>Adds a key. If the key already exists, its value is replaced.</summary>
    public void Insert(TKey key, TValue value)
    {
        _root = Insert(_root, key, value);
    }

    private Node Insert(Node? node, TKey key, TValue value)
    {
        if (node is null)
        {
            Count++;
            return new Node(key, value);
        }

        int compare = key.CompareTo(node.Key);

        if (compare < 0)
            node.Left = Insert(node.Left, key, value);
        else if (compare > 0)
            node.Right = Insert(node.Right, key, value);
        else
        {
            node.Value = value;   // same key: replace, no rebalancing needed
            return node;
        }

        return Rebalance(node);
    }

    // ---------- lookup ----------

    public bool TryGetValue(TKey key, out TValue? value)
    {
        var node = _root;
        while (node is not null)
        {
            int compare = key.CompareTo(node.Key);
            if (compare == 0)
            {
                value = node.Value;
                return true;
            }
            node = compare < 0 ? node.Left : node.Right;
        }

        value = default;
        return false;
    }

    public bool ContainsKey(TKey key) => TryGetValue(key, out _);

    // ---------- range query ----------

    /// <summary>
    /// Returns all entries with from &lt;= key &lt;= to, in ascending key order.
    /// Skips whole branches that cannot contain results.
    /// </summary>
    public List<KeyValuePair<TKey, TValue>> Range(TKey from, TKey to)
    {
        var results = new List<KeyValuePair<TKey, TValue>>();
        CollectRange(_root, from, to, results);
        return results;
    }

    private static void CollectRange(
        Node? node, TKey from, TKey to, List<KeyValuePair<TKey, TValue>> results)
    {
        if (node is null) return;

        // Only look left if there could be keys >= from down there
        if (from.CompareTo(node.Key) < 0)
            CollectRange(node.Left, from, to, results);

        if (from.CompareTo(node.Key) <= 0 && to.CompareTo(node.Key) >= 0)
            results.Add(new KeyValuePair<TKey, TValue>(node.Key, node.Value));

        // Only look right if there could be keys <= to down there
        if (to.CompareTo(node.Key) > 0)
            CollectRange(node.Right, from, to, results);
    }

    // ---------- newest N entries ----------

    /// <summary>Returns the last (largest-key) N entries in ascending order.</summary>
    public List<KeyValuePair<TKey, TValue>> Last(int count)
    {
        var results = new List<KeyValuePair<TKey, TValue>>();
        CollectLast(_root, count, results);
        results.Reverse();   // collected newest-first, return oldest-first
        return results;
    }

    private static void CollectLast(
        Node? node, int count, List<KeyValuePair<TKey, TValue>> results)
    {
        if (node is null || results.Count >= count) return;

        CollectLast(node.Right, count, results);   // biggest keys first

        if (results.Count < count)
            results.Add(new KeyValuePair<TKey, TValue>(node.Key, node.Value));

        CollectLast(node.Left, count, results);
    }

    // ---------- everything, in order ----------

    public List<KeyValuePair<TKey, TValue>> InOrder()
    {
        var results = new List<KeyValuePair<TKey, TValue>>();
        CollectInOrder(_root, results);
        return results;
    }

    private static void CollectInOrder(
        Node? node, List<KeyValuePair<TKey, TValue>> results)
    {
        if (node is null) return;
        CollectInOrder(node.Left, results);
        results.Add(new KeyValuePair<TKey, TValue>(node.Key, node.Value));
        CollectInOrder(node.Right, results);
    }
}