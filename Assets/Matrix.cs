namespace Nonno.Assets;

public static class Matrix
{
    public static double[,] Transpose(double[,] A)
    {
        double[,] AT = new double[A.GetLength(1), A.GetLength(0)];

        for (int i = 0; i < A.GetLength(1); i++)
        {
            for (int j = 0; j < A.GetLength(0); j++)
            {
                AT[i, j] = A[j, i];
            }
        }

        return AT;
    }

    public static double[,] Multiple(double[,] A, double[,] B)
    {
        double[,] product = new double[A.GetLength(0), B.GetLength(1)];

        for (int i = 0; i < A.GetLength(0); i++)
        {
            for (int j = 0; j < B.GetLength(1); j++)
            {
                for (int k = 0; k < A.GetLength(1); k++)
                {
                    product[i, j] += A[i, k] * B[k, j];
                }
            }
        }

        return product;
    }

    public static double[,] Inverse(double[,] A)
    {
        int n = A.GetLength(0);
        int m = A.GetLength(1);

        double[,] invA = new double[n, m];

        if (n == m)
        {
            int max;
            double tmp;

            for (int j = 0; j < n; j++)
            {
                for (int i = 0; i < n; i++)
                {
                    invA[j, i] = (i == j) ? 1 : 0;
                }
            }

            for (int k = 0; k < n; k++)
            {
                max = k;
                for (int j = k + 1; j < n; j++)
                {
                    if (Math.Abs(A[j, k]) > Math.Abs(A[max, k]))
                    {
                        max = j;
                    }
                }

                if (max != k)
                {
                    for (int i = 0; i < n; i++)
                    {
                        // “ü—Ís—ñ‘¤
                        tmp = A[max, i];
                        A[max, i] = A[k, i];
                        A[k, i] = tmp;
                        // ’PˆÊs—ñ‘¤
                        tmp = invA[max, i];
                        invA[max, i] = invA[k, i];
                        invA[k, i] = tmp;
                    }
                }

                tmp = A[k, k];

                for (int i = 0; i < n; i++)
                {
                    A[k, i] /= tmp;
                    invA[k, i] /= tmp;
                }

                for (int j = 0; j < n; j++)
                {
                    if (j != k)
                    {
                        tmp = A[j, k] / A[k, k];
                        for (int i = 0; i < n; i++)
                        {
                            A[j, i] = A[j, i] - A[k, i] * tmp;
                            invA[j, i] = invA[j, i] - invA[k, i] * tmp;
                        }
                    }
                }
            }

            //‹ts—ñ‚ªŒvŽZ‚Å‚«‚È‚©‚Á‚½Žž‚Ì‘[’u
            for (int j = 0; j < n; j++)
            {
                for (int i = 0; i < n; i++)
                {
                    if (double.IsNaN(invA[j, i]))
                    {
                        Console.WriteLine("Error : Unable to compute inverse matrix");
                        invA[j, i] = 0;//‚±‚±‚Å‚ÍC‚Æ‚è‚ ‚¦‚¸ƒ[ƒ‚É’u‚«Š·‚¦‚é‚±‚Æ‚É‚·‚é
                    }
                }
            }

            return invA;
        }
        else
        {
            Console.WriteLine("Error : It is not a square matrix");
            return invA;
        }
    }
}