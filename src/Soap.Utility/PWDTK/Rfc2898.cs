namespace Soap.Utility.PWDTK
{
    using System;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    ///     Implementation of the Rfc2898 PBKDF2 specification located here http://www.ietf.org/rfc/rfc2898.txt using
    ///     HMACSHA512 as the underlying PRF
    ///     Made by thashiznets@yahoo.com.au
    ///     v1.0.0.0
    /// </summary>
    public class Rfc2898
    {
        //Minimum rcommended itereations in Rfc2898
        public const int CMinIterations = 1000;

        //Minimum recommended salt length in Rfc2898
        public const int CMinSaltLength = 8;

        //I made the variable names match the definition in RFC2898 - PBKDF2 where possible, so you can trace the code functionality back to the specification
        private readonly HMACSHA512 _hmacsha512Obj;

        private readonly int c;

        private readonly int hLen;

        private readonly byte[] P;

        private readonly byte[] S;

        private int dkLen;

        /// <summary>
        ///     Rfc2898 constructor to create Rfc2898 object ready to perform Rfc2898 functionality
        /// </summary>
        /// <param name="password">The Password to be hashed and is also the HMAC key</param>
        /// <param name="salt">Salt to be concatenated with the password</param>
        /// <param name="iterations">Number of iterations to perform HMACSHA Hashing for PBKDF2</param>
        public Rfc2898(byte[] password, byte[] salt, int iterations)
        {
            if (iterations < CMinIterations)
            {
                throw new IterationsLessThanRecommended();
            }

            if (salt.Length < CMinSaltLength)
            {
                throw new SaltLessThanRecommended();
            }

            this._hmacsha512Obj = new HMACSHA512(password);
            this.hLen = this._hmacsha512Obj.HashSize / 8;
            this.P = password;
            this.S = salt;
            this.c = iterations;
        }

        /// <summary>
        ///     Rfc2898 constructor to create Rfc2898 object ready to perform Rfc2898 functionality
        /// </summary>
        /// <param name="password">The Password to be hashed and is also the HMAC key</param>
        /// <param name="salt">Salt to be concatenated with the password</param>
        /// <param name="iterations">Number of iterations to perform HMACSHA Hashing for PBKDF2</param>
        public Rfc2898(string password, byte[] salt, int iterations)
            : this(new UTF8Encoding(false).GetBytes(password), salt, iterations)
        {
        }

        /// <summary>
        ///     Rfc2898 constructor to create Rfc2898 object ready to perform Rfc2898 functionality
        /// </summary>
        /// <param name="password">The Password to be hashed and is also the HMAC key</param>
        /// <param name="salt">Salt to be concatenated with the password</param>
        /// <param name="iterations">Number of iterations to perform HMACSHA Hashing for PBKDF2</param>
        public Rfc2898(string password, string salt, int iterations)
            : this(new UTF8Encoding(false).GetBytes(password), new UTF8Encoding(false).GetBytes(salt), iterations)
        {
        }

        /// <summary>
        ///     A static publicly exposed version of GetDerivedKeyBytes_PBKDF2_HMACSHA512 which matches the exact specification in
        ///     Rfc2898 PBKDF2 using HMACSHA512
        /// </summary>
        /// <param name="P">Password passed as a Byte Array</param>
        /// <param name="S">Salt passed as a Byte Array</param>
        /// <param name="c">Iterations to perform the underlying PRF over</param>
        /// <param name="dkLen">Length of Bytes to return, an AES 256 key wold require 32 Bytes</param>
        /// <returns>Derived Key in Byte Array form ready for use by chosen encryption function</returns>
        public static byte[] PBKDF2(byte[] P, byte[] S, int c, int dkLen)
        {
            var rfcObj = new Rfc2898(P, S, c);
            return rfcObj.GetDerivedKeyBytes_PBKDF2_HMACSHA512(dkLen);
        }

        /// <summary>
        ///     Derive Key Bytes using PBKDF2 specification listed in Rfc2898 and HMACSHA512 as the underlying PRF (Psuedo Random
        ///     Function)
        /// </summary>
        /// <param name="keyLength">Length in Bytes of Derived Key</param>
        /// <returns>Derived Key</returns>
        public byte[] GetDerivedKeyBytes_PBKDF2_HMACSHA512(int keyLength)
        {
            //no need to throw exception for dkLen too long as per spec because dkLen cannot be larger than Int32.MaxValue so not worth the overhead to check
            this.dkLen = keyLength;

            var l = Math.Ceiling((double)this.dkLen / this.hLen);

            var finalBlock = new byte[0];

            for (var i = 1; i <= l; i++)
                //Concatenate each block from F into the final block (T_1..T_l)
                finalBlock = pMergeByteArrays(finalBlock, F(this.P, this.S, this.c, i));

            //returning DK note r not used as dkLen bytes of the final concatenated block returned rather than <0...r-1> substring of final intermediate block + prior blocks as per spec
            return finalBlock.Take(this.dkLen).ToArray();
        }

        //Main Function F as defined in Rfc2898 PBKDF2 spec
        private byte[] F(byte[] P, byte[] S, int c, int i)
        {
            //Salt and Block number Int(i) concatenated as per spec
            var Si = pMergeByteArrays(S, INT(i));

            //Initial hash (U_1) using password and salt concatenated with Int(i) as per spec
            var temp = PRF(P, Si);

            //Output block filled with initial hash value or U_1 as per spec
            var U_c = temp;

            for (var C = 1; C < c; C++)
            {
                //rehashing the password using the previous hash value as salt as per spec
                temp = PRF(P, temp);

                for (var j = 0; j < temp.Length; j++)
                    //xor each byte of the each hash block with each byte of the output block as per spec
                    U_c[j] ^= temp[j];
            }

            //return a T_i block for concatenation to create the final block as per spec
            return U_c;
        }

        //This method returns the 4 octet encoded Int32 with most significant bit first as per spec
        private byte[] INT(int i)
        {
            var I = BitConverter.GetBytes(i);

            //Make sure most significant bit is first
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(I);
            }

            return I;
        }

        //Merge two arrays into a new array
        private byte[] pMergeByteArrays(byte[] source1, byte[] source2)
        {
            //Most efficient way to merge two arrays this according to http://stackoverflow.com/questions/415291/best-way-to-combine-two-or-more-byte-arrays-in-c-sharp
            var buffer = new byte[source1.Length + source2.Length];
            Buffer.BlockCopy(source1, 0, buffer, 0, source1.Length);
            Buffer.BlockCopy(source2, 0, buffer, source1.Length, source2.Length);

            return buffer;
        }

        //PRF function as defined in Rfc2898 PBKDF2 spec
        private byte[] PRF(byte[] P, byte[] S) =>
            //HMACSHA512 Hashing, better than the HMACSHA1 in Microsofts implementation ;)
            this._hmacsha512Obj.ComputeHash(pMergeByteArrays(P, S));
    }

    #region Rfc2898 Custom Exceptions

    public class IterationsLessThanRecommended : Exception
    {
        public IterationsLessThanRecommended()
            : base("Iteration count is less than the 1000 recommended in Rfc2898")
        {
        }
    }

    public class SaltLessThanRecommended : Exception
    {
        public SaltLessThanRecommended()
            : base("Salt is less than the 8 byte size recommended in Rfc2898")
        {
        }
    }

    #endregion
}