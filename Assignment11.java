import java.util.HashMap;
import java.util.Map;

public class Assignment11b {
    
    // Part A: Recursive Fibonacci with counter
    static class RecursiveFib {
        static int recursiveCallCount = 0;
        
        static int fib(int n) {
            recursiveCallCount++;
            if (n == 0) return 0;
            if (n == 1) return 1;
            return fib(n - 1) + fib(n - 2);
        }
    }
    
    // Part B: Memoized Fibonacci
    static class MemoizedFib {
        static int recursiveCallCount = 0;
        static Map<Integer, Integer> memo = new HashMap<>();
        
        static int fib(int n) {
            recursiveCallCount++;
            if (memo.containsKey(n)) {
                return memo.get(n);
            }
            
            int result;
            if (n == 0) result = 0;
            else if (n == 1) result = 1;
            else result = fib(n - 1) + fib(n - 2);
            
            memo.put(n, result);
            return result;
        }
    }
    
    // Part C: Iterative Fibonacci with two variables
    static class IterativeFib {
        static int fib(int n) {
            if (n == 0) return 0;
            if (n == 1) return 1;
            
            int prev2 = 0;
            int prev1 = 1;
            int current = 0;
            
            for (int i = 2; i <= n; i++) {
                current = prev1 + prev2;
                prev2 = prev1;
                prev1 = current;
            }
            return current;
        }
    }
    
    // LCS Brute Force
    static class BruteForceLCS {
        static int operationCount = 0;
        
        static int lcs(String s1, String s2, int m, int n) {
            operationCount++;
            
            if (m == 0 || n == 0) {
                return 0;
            }
            
            if (s1.charAt(m - 1) == s2.charAt(n - 1)) {
                return 1 + lcs(s1, s2, m - 1, n - 1);
            } else {
                return Math.max(lcs(s1, s2, m, n - 1), 
                               lcs(s1, s2, m - 1, n));
            }
        }
    }
    
    // LCS Memoized
    static class MemoizedLCS {
        static int operationCount = 0;
        static Map<String, Integer> memo = new HashMap<>();
        
        static int lcs(String s1, String s2, int m, int n) {
            operationCount++;
            
            String key = m + "," + n;
            if (memo.containsKey(key)) {
                return memo.get(key);
            }
            
            int result;
            if (m == 0 || n == 0) {
                result = 0;
            } else if (s1.charAt(m - 1) == s2.charAt(n - 1)) {
                result = 1 + lcs(s1, s2, m - 1, n - 1);
            } else {
                result = Math.max(lcs(s1, s2, m, n - 1), 
                                 lcs(s1, s2, m - 1, n));
            }
            
            memo.put(key, result);
            return result;
        }
    }
    
    public static void main(String[] args) {
        System.out.println("=== FIBONACCI IMPLEMENTATIONS ===");
        
        // Test Fibonacci implementations
        int n = 30;
        
        // Part A: Recursive
        RecursiveFib.recursiveCallCount = 0;
        long startTime = System.nanoTime();
        int result1 = RecursiveFib.fib(n);
        long endTime = System.nanoTime();
        System.out.println("Recursive Fib(" + n + ") = " + result1);
        System.out.println("Recursive calls: " + RecursiveFib.recursiveCallCount);
        System.out.println("Time: " + (endTime - startTime) / 1000000 + " ms");
        
        // Part B: Memoized
        MemoizedFib.recursiveCallCount = 0;
        MemoizedFib.memo.clear();
        startTime = System.nanoTime();
        int result2 = MemoizedFib.fib(n);
        endTime = System.nanoTime();
        System.out.println("\nMemoized Fib(" + n + ") = " + result2);
        System.out.println("Recursive calls: " + MemoizedFib.recursiveCallCount);
        System.out.println("Time: " + (endTime - startTime) / 1000000 + " ms");
        
        // Part C: Iterative
        startTime = System.nanoTime();
        int result3 = IterativeFib.fib(n);
        endTime = System.nanoTime();
        System.out.println("\nIterative Fib(" + n + ") = " + result3);
        System.out.println("Time: " + (endTime - startTime) / 1000000 + " ms");
        
        System.out.println("\n=== LCS IMPLEMENTATIONS ===");
        
        // Test LCS implementations
        String s1 = "ABCDGH";
        String s2 = "AEDFHR";
        
        // Brute Force LCS
        BruteForceLCS.operationCount = 0;
        startTime = System.nanoTime();
        int lcsResult1 = BruteForceLCS.lcs(s1, s2, s1.length(), s2.length());
        endTime = System.nanoTime();
        System.out.println("Brute Force LCS(\"" + s1 + "\", \"" + s2 + "\") = " + lcsResult1);
        System.out.println("Operations: " + BruteForceLCS.operationCount);
        System.out.println("Time: " + (endTime - startTime) / 1000000 + " ms");
        
        // Memoized LCS
        MemoizedLCS.operationCount = 0;
        MemoizedLCS.memo.clear();
        startTime = System.nanoTime();
        int lcsResult2 = MemoizedLCS.lcs(s1, s2, s1.length(), s2.length());
        endTime = System.nanoTime();
        System.out.println("\nMemoized LCS(\"" + s1 + "\", \"" + s2 + "\") = " + lcsResult2);
        System.out.println("Operations: " + MemoizedLCS.operationCount);
        System.out.println("Time: " + (endTime - startTime) / 1000000 + " ms");
        
        // Test with larger strings
        System.out.println("\n=== LARGER STRING TEST ===");
        String large1 = "ABCDEFGHIJKLMNOP";
        String large2 = "ACEGIKMOQSUWY";
        
        BruteForceLCS.operationCount = 0;
        startTime = System.nanoTime();
        int largeLcs1 = BruteForceLCS.lcs(large1, large2, large1.length(), large2.length());
        endTime = System.nanoTime();
        System.out.println("Brute Force Large LCS = " + largeLcs1);
        System.out.println("Operations: " + BruteForceLCS.operationCount);
        System.out.println("Time: " + (endTime - startTime) / 1000000 + " ms");
        
        MemoizedLCS.operationCount = 0;
        MemoizedLCS.memo.clear();
        startTime = System.nanoTime();
        int largeLcs2 = MemoizedLCS.lcs(large1, large2, large1.length(), large2.length());
        endTime = System.nanoTime();
        System.out.println("\nMemoized Large LCS = " + largeLcs2);
        System.out.println("Operations: " + MemoizedLCS.operationCount);
        System.out.println("Time: " + (endTime - startTime) / 1000000 + " ms");
    }
}

/*
STATUS REPORT:
- Part A: COMPLETED - Recursive Fibonacci with call counter
- Part B: COMPLETED - Memoized Fibonacci with call counter  
- Part C: COMPLETED - Iterative Fibonacci with two variables
- LCS: COMPLETED - Both brute force and memoized versions

ANSWERS:
1. Recursive calls for Fib(30): 2,692,537 calls
2. Memoized calls for Fib(30): 59 calls (2n-1 for first computation)
3. The brute force version recalculates the same subproblems exponentially many times.
   For Fib(30), it calculates Fib(1) over 832,040 times. Dynamic programming eliminates
   this redundancy by storing previously computed results, reducing time complexity
   from O(2^n) to O(n).
*/