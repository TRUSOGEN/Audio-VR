using System;

/// <summary>
/// Provides the candidate four-condition orders and validates that each formal condition appears once.
/// It deliberately does not silently fill failed allocation constraints.
/// </summary>
public static class RouteAllocationPlanner
{
    private static readonly string[][] CandidateOrders =
    {
        new[] { "NS_Q", "NS_N", "S_N", "S_Q" },
        new[] { "NS_N", "S_N", "NS_Q", "S_Q" },
        new[] { "S_Q", "S_N", "NS_N", "NS_Q" },
        new[] { "S_N", "NS_Q", "S_Q", "NS_N" }
    };

    public static string[] GetCandidateOrder(int participantIndex)
    {
        int index = Math.Abs(participantIndex) % CandidateOrders.Length;
        string[] source = CandidateOrders[index];
        string[] copy = new string[source.Length];
        Array.Copy(source, copy, source.Length);
        return copy;
    }

    public static bool ValidateOrder(string[] order, out string errorCode)
    {
        errorCode = "";
        if (order == null || order.Length != 4)
        {
            errorCode = "ALLOCATION_ORDER_LENGTH_INVALID";
            return false;
        }

        string[] expected = { "NS_Q", "NS_N", "S_Q", "S_N" };
        for (int i = 0; i < expected.Length; i++)
        {
            int count = 0;
            for (int j = 0; j < order.Length; j++)
            {
                if (string.Equals(expected[i], order[j], StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }
            }

            if (count != 1)
            {
                errorCode = "ALLOCATION_CONDITION_DUPLICATE_OR_MISSING_" + expected[i];
                return false;
            }
        }

        return true;
    }
}
