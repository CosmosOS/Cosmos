using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Security.Cryptography;
using Indy.IL2CPU.Assembler;
using System.Diagnostics;


namespace Cosmos.IL2CPU
{
    public static class MethodInfoLabelGenerator
    {

        public static string GenerateLabelName( MethodBase aMethod )
        {
            string xResult = DataMember.FilterStringForIncorrectChars( GenerateFullName( aMethod ) );
            xResult = GenerateLabelFromFullName( xResult );
            return xResult;
        }

        public static string GenerateLabelFromFullName( string xResult )
        {
            if( xResult.Length > 245 )
            {
                using( var xHash = MD5.Create() )
                {
                    xResult = xHash.ComputeHash(
                        Encoding.Default.GetBytes( xResult ) ).Aggregate( "_", ( r, x ) => r + x.ToString( "X2" ) );
                }
            }
            return xResult;
        }


        //public static string GenerateLabelName(MethodBase aMethod)
        //{
        //    return GenerateLabelNameInt(aMethod).ToString();
        //}

        //TODO Unicode support
        public static ulong GenerateLabelNameInt( MethodBase aMethod )
        {



            string xResult = DataMember.FilterStringForIncorrectChars( GenerateFullName( aMethod ) );


            var encoding = new System.Text.UTF8Encoding();

            byte[] bytes = encoding.GetBytes( xResult );


            var hash = Elf64.Compute( 0, 0, bytes );

            //  Debug.WriteLine(" hash: " + hash.ToString() + "  Output : " + xResult ); 


            return hash;
            //ulong result , g = 0 ; 

            //foreach ( char ch in xResult.ToCharArray())
            //{
            //    result = (result << 4) + (byte)ch;
            //    result++;
            //    g = result & 0xF0000000;
            //    if (g > 0  ) // wrap
            //        result = g >> 24;

            //    result &= ~g; 

        }
        //            unsigned long hash(char *name)
        //{
        // unsigned long h = 0, g;

        // while ( *name ) {
        //  h = ( h << 4 ) + *name++;
        //  if ( g = h & 0xF0000000 )
        //  h ^= g >> 24;
        //  h &= ~g;
        // }

        // return h;
        //}



        public static string GetFullName( Type aType )
        {
            if( aType.IsGenericParameter )
            {
                return aType.Name;
            }
            var xSB = new StringBuilder();
            if( aType.IsArray )
            {
                xSB.Append( GetFullName( aType.GetElementType() ) );
                xSB.Append( "[" );
                int xRank = aType.GetArrayRank();
                while( xRank > 1 )
                {
                    xSB.Append( "," );
                    xRank--;
                }
                xSB.Append( "]" );
                return xSB.ToString();
            }
            if( aType.IsByRef && aType.HasElementType )
            {
                return "&" + GetFullName( aType.GetElementType() );
            }
            if( aType.IsGenericType )
            {
                xSB.Append( aType.GetGenericTypeDefinition().FullName );
            }
            else
            {
                xSB.Append( aType.FullName );
            }
            if( aType.IsGenericType )
            {
                xSB.Append( "<" );
                var xArgs = aType.GetGenericArguments();
                for( int i = 0; i < xArgs.Length - 1; i++ )
                {
                    xSB.Append( GetFullName( xArgs[ i ] ) );
                    xSB.Append( ", " );
                } if( xArgs.Length == 0 ) { Console.Write( "" ); }
                xSB.Append( GetFullName( xArgs.Last() ) );
                xSB.Append( ">" );
            }
            //xSB.Append(", ");
            //xSB.Append(aType.Assembly.FullName);
            return xSB.ToString();
        }

        private static string GenerateFullName( MethodBase aMethod )
        {
            if( aMethod == null )
            {
                throw new ArgumentNullException( "aMethod" );
            }
            var xBuilder = new StringBuilder( 256 );
            var xParts = aMethod.ToString().Split( ' ' );
            var xParts2 = xParts.Skip( 1 ).ToArray();
            var xMethodInfo = aMethod as System.Reflection.MethodInfo;
            if( xMethodInfo != null )
            {
                xBuilder.Append( GetFullName( xMethodInfo.ReturnType ) );
            }
            else
            {
                var xCtor = aMethod as ConstructorInfo;
                if( xCtor != null )
                {
                    xBuilder.Append( typeof( void ).FullName );
                }
                else
                {
                    xBuilder.Append( xParts[ 0 ] );
                }
            }
            xBuilder.Append( "  " );
            xBuilder.Append( GetFullName( aMethod.DeclaringType ) );
            xBuilder.Append( "." );
            xBuilder.Append( aMethod.Name );
            if( aMethod.IsGenericMethod || aMethod.IsGenericMethodDefinition )
            {
                var xGenArgs = aMethod.GetGenericArguments();
                if( xGenArgs.Length > 0 )
                {
                    xBuilder.Append( "<" );
                    for( int i = 0; i < xGenArgs.Length - 1; i++ )
                    {
                        xBuilder.Append( GetFullName( xGenArgs[ i ] ) );
                        xBuilder.Append( ", " );
                    }
                    xBuilder.Append( GetFullName( xGenArgs.Last() ) );
                    xBuilder.Append( ">" );
                }
            }
            xBuilder.Append( "(" );
            var xParams = aMethod.GetParameters();
            for( var i = 0; i < xParams.Length; i++ )
            {
                if( xParams[ i ].Name == "aThis" && i == 0 )
                {
                    continue;
                }
                xBuilder.Append( GetFullName( xParams[ i ].ParameterType ) );
                if( i < ( xParams.Length - 1 ) )
                {
                    xBuilder.Append( ", " );
                }
            }
            xBuilder.Append( ")" );
            return String.Intern( xBuilder.ToString() );
        }

        public static string GetFullName( FieldInfo aField )
        {
            return GetFullName( aField.FieldType ) + " " + GetFullName( aField.DeclaringType ) + "." + aField.Name;
            //var xSB = new StringBuilder(aField.FieldType.FullName.Length + 1 + aField.DeclaringType.FullName.Length + 1 + aField.Name);
            //xSB.Append(aField.FieldType.FullName);
            //xSB.Append(" ");
            //xSB.Append(aField.DeclaringType.FullName);
            //xSB.Append(".");
            //xSB.Append(aField.Name);
            //return String.Intern(xSB.ToString());
        }
    }
}
