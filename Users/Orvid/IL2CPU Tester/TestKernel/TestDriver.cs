using System;

namespace IL2CPUTester
{
    public class TestDriver
    {
        public static void RunTests()
        {
            #region Tests
            if (Tests.test_0_multidym_array_with_negative_lower_bound() != 0)
            {
                Console.WriteLine("Test 'test_0_multidym_array_with_negative_lower_bound' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_multidym_array_with_negative_lower_bound().ToString() + "'");
            }
            if (Tests.test_0_invalid_new_multi_dym_array_size() != 0)
            {
                Console.WriteLine("Test 'test_0_invalid_new_multi_dym_array_size' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_invalid_new_multi_dym_array_size().ToString() + "'");
            }
            if (Tests.test_0_primitive_array_cast() != 0)
            {
                Console.WriteLine("Test 'test_0_primitive_array_cast' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_primitive_array_cast().ToString() + "'");
            }
            if (Tests.test_0_intptr_array_cast() != 0)
            {
                Console.WriteLine("Test 'test_0_intptr_array_cast' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_intptr_array_cast().ToString() + "'");
            }
            if (Tests.test_0_sqrt_precision_and_spill() != 0)
            {
                Console.WriteLine("Test 'test_0_sqrt_precision_and_spill' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_sqrt_precision_and_spill().ToString() + "'");
            }
            if (Tests.test_0_div_precision_and_spill() != 0)
            {
                Console.WriteLine("Test 'test_0_div_precision_and_spill' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_div_precision_and_spill().ToString() + "'");
            }
            if (Tests.test_0_sqrt_nan() != 0)
            {
                Console.WriteLine("Test 'test_0_sqrt_nan' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_sqrt_nan().ToString() + "'");
            }
            if (Tests.test_0_sin_nan() != 0)
            {
                Console.WriteLine("Test 'test_0_sin_nan' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_sin_nan().ToString() + "'");
            }
            if (Tests.test_0_cos_nan() != 0)
            {
                Console.WriteLine("Test 'test_0_cos_nan' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_cos_nan().ToString() + "'");
            }
            if (Tests.test_0_tan_nan() != 0)
            {
                Console.WriteLine("Test 'test_0_tan_nan' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_tan_nan().ToString() + "'");
            }
            if (Tests.test_0_atan_nan() != 0)
            {
                Console.WriteLine("Test 'test_0_atan_nan' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_atan_nan().ToString() + "'");
            }
            if (Tests.test_0_min() != 0)
            {
                Console.WriteLine("Test 'test_0_min' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_min().ToString() + "'");
            }
            if (Tests.test_0_max() != 0)
            {
                Console.WriteLine("Test 'test_0_max' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_max().ToString() + "'");
            }
            if (Tests.test_0_min_un() != 0)
            {
                Console.WriteLine("Test 'test_0_min_un' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_min_un().ToString() + "'");
            }
            if (Tests.test_0_max_un() != 0)
            {
                Console.WriteLine("Test 'test_0_max_un' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_max_un().ToString() + "'");
            }
            if (Tests.test_0_abs() != 0)
            {
                Console.WriteLine("Test 'test_0_abs' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_abs().ToString() + "'");
            }
            if (Tests.test_0_round() != 0)
            {
                Console.WriteLine("Test 'test_0_round' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_round().ToString() + "'");
            }
            if (Tests.test_0_regstruct() != 0)
            {
                Console.WriteLine("Test 'test_0_regstruct' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_regstruct().ToString() + "'");
            }
            if (Tests.test_0_reg_return() != 0)
            {
                Console.WriteLine("Test 'test_0_reg_return' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_reg_return().ToString() + "'");
            }
            if (Tests.test_0_spill_regs() != 0)
            {
                Console.WriteLine("Test 'test_0_spill_regs' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_spill_regs().ToString() + "'");
            }
            if (Tests.test_0_spill() != 0)
            {
                Console.WriteLine("Test 'test_0_spill' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_spill().ToString() + "'");
            }
            if (Tests.test_0_spill_void() != 0)
            {
                Console.WriteLine("Test 'test_0_spill_void' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_spill_void().ToString() + "'");
            }
            if (Tests.test_0_spill_ret() != 0)
            {
                Console.WriteLine("Test 'test_0_spill_ret' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_spill_ret().ToString() + "'");
            }
            if (Tests.test_0_struct_ret() != 0)
            {
                Console.WriteLine("Test 'test_0_struct_ret' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_struct_ret().ToString() + "'");
            }
            if (Tests.test_0_TestSingle() != 0)
            {
                Console.WriteLine("Test 'test_0_TestSingle' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_TestSingle().ToString() + "'");
            }
            if (Tests.test_0_pass_spill() != 0)
            {
                Console.WriteLine("Test 'test_0_pass_spill' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_pass_spill().ToString() + "'");
            }
            if (Tests.test_0_pass_spill_big() != 0)
            {
                Console.WriteLine("Test 'test_0_pass_spill_big' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_pass_spill_big().ToString() + "'");
            }
            if (Tests.test_0_pass_struct_spill() != 0)
            {
                Console.WriteLine("Test 'test_0_pass_struct_spill' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_pass_struct_spill().ToString() + "'");
            }
            if (Tests.test_0_pass_struct_spill_big() != 0)
            {
                Console.WriteLine("Test 'test_0_pass_struct_spill_big' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_pass_struct_spill_big().ToString() + "'");
            }
            if (Tests.test_0_pass_ret_big_struct() != 0)
            {
                Console.WriteLine("Test 'test_0_pass_ret_big_struct' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_pass_ret_big_struct().ToString() + "'");
            }
            if (Tests.test_0_pass_spill_struct_spill() != 0)
            {
                Console.WriteLine("Test 'test_0_pass_spill_struct_spill' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_pass_spill_struct_spill().ToString() + "'");
            }
            if (Tests.test_0_pass_spill_struct_spill_big() != 0)
            {
                Console.WriteLine("Test 'test_0_pass_spill_struct_spill_big' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_pass_spill_struct_spill_big().ToString() + "'");
            }
            if (Tests.test_0_pass_long_odd() != 0)
            {
                Console.WriteLine("Test 'test_0_pass_long_odd' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_pass_long_odd().ToString() + "'");
            }
            if (Tests.test_0_pass_double_ret_float() != 0)
            {
                Console.WriteLine("Test 'test_0_pass_double_ret_float' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_pass_double_ret_float().ToString() + "'");
            }
            if (Tests.test_0_pass_float_ret_double() != 0)
            {
                Console.WriteLine("Test 'test_0_pass_float_ret_double' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_pass_float_ret_double().ToString() + "'");
            }
            if (Tests.test_0_sealed_class_devirt_right_method() != 0)
            {
                Console.WriteLine("Test 'test_0_sealed_class_devirt_right_method' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_sealed_class_devirt_right_method().ToString() + "'");
            }
            if (Tests.test_0_sealed_method_devirt_right_method() != 0)
            {
                Console.WriteLine("Test 'test_0_sealed_method_devirt_right_method' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_sealed_method_devirt_right_method().ToString() + "'");
            }
            if (Tests.test_0_sealed_class_devirt_right_method_using_delegates() != 0)
            {
                Console.WriteLine("Test 'test_0_sealed_class_devirt_right_method_using_delegates' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_sealed_class_devirt_right_method_using_delegates().ToString() + "'");
            }
            if (Tests.test_0_sealed_method_devirt_right_method_using_delegates() != 0)
            {
                Console.WriteLine("Test 'test_0_sealed_method_devirt_right_method_using_delegates' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_sealed_method_devirt_right_method_using_delegates().ToString() + "'");
            }
            if (Tests.test_0_delegate_over_static_method_devirtualize_ok() != 0)
            {
                Console.WriteLine("Test 'test_0_delegate_over_static_method_devirtualize_ok' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_delegate_over_static_method_devirtualize_ok().ToString() + "'");
            }
            if (Tests.test_0_npe_still_happens() != 0)
            {
                Console.WriteLine("Test 'test_0_npe_still_happens' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_npe_still_happens().ToString() + "'");
            }
            if (Tests.test_10_create() != 10)
            {
                Console.WriteLine("Test 'test_10_create' didn't return expected value. Expected: '10' Got: '" + Tests.test_10_create().ToString() + "'");
            }
            if (Tests.test_0_unset_value() != 0)
            {
                Console.WriteLine("Test 'test_0_unset_value' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_unset_value().ToString() + "'");
            }
            if (Tests.test_3_set_value() != 3)
            {
                Console.WriteLine("Test 'test_3_set_value' didn't return expected value. Expected: '3' Got: '" + Tests.test_3_set_value().ToString() + "'");
            }
            if (Tests.test_0_char_array_1() != 0)
            {
                Console.WriteLine("Test 'test_0_char_array_1' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_char_array_1().ToString() + "'");
            }
            if (Tests.test_0_char_array_2() != 0)
            {
                Console.WriteLine("Test 'test_0_char_array_2' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_char_array_2().ToString() + "'");
            }
            if (Tests.test_0_char_array_3() != 0)
            {
                Console.WriteLine("Test 'test_0_char_array_3' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_char_array_3().ToString() + "'");
            }
            if (Tests.test_0_byte_array() != 0)
            {
                Console.WriteLine("Test 'test_0_byte_array' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_byte_array().ToString() + "'");
            }
            if (Tests.test_0_set_after_shift() != 0)
            {
                Console.WriteLine("Test 'test_0_set_after_shift' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_set_after_shift().ToString() + "'");
            }
            if (Tests.test_0_newarr_emulation() != 0)
            {
                Console.WriteLine("Test 'test_0_newarr_emulation' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_newarr_emulation().ToString() + "'");
            }
            if (Tests.test_1_bit_index() != 1)
            {
                Console.WriteLine("Test 'test_1_bit_index' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_bit_index().ToString() + "'");
            }
            if (Tests.test_2_regalloc() != 2)
            {
                Console.WriteLine("Test 'test_2_regalloc' didn't return expected value. Expected: '2' Got: '" + Tests.test_2_regalloc().ToString() + "'");
            }
            if (Tests.test_0_stelemref_1() != 0)
            {
                Console.WriteLine("Test 'test_0_stelemref_1' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_stelemref_1().ToString() + "'");
            }
            if (Tests.test_0_stelemref_2() != 0)
            {
                Console.WriteLine("Test 'test_0_stelemref_2' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_stelemref_2().ToString() + "'");
            }
            if (Tests.test_0_stelemref_3() != 0)
            {
                Console.WriteLine("Test 'test_0_stelemref_3' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_stelemref_3().ToString() + "'");
            }
            if (Tests.test_0_stelemref_4() != 0)
            {
                Console.WriteLine("Test 'test_0_stelemref_4' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_stelemref_4().ToString() + "'");
            }
            if (Tests.test_0_arrays() != 0)
            {
                Console.WriteLine("Test 'test_0_arrays' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_arrays().ToString() + "'");
            }
            if (Tests.test_0_multi_dimension_arrays() != 0)
            {
                Console.WriteLine("Test 'test_0_multi_dimension_arrays' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_multi_dimension_arrays().ToString() + "'");
            }
            if (Tests.test_100_3_dimensional_arrays() != 100)
            {
                Console.WriteLine("Test 'test_100_3_dimensional_arrays' didn't return expected value. Expected: '100' Got: '" + Tests.test_100_3_dimensional_arrays().ToString() + "'");
            }
            if (Tests.test_0_bug_71454() != 0)
            {
                Console.WriteLine("Test 'test_0_bug_71454' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_bug_71454().ToString() + "'");
            }
            if (Tests.test_0_interface_array_cast() != 0)
            {
                Console.WriteLine("Test 'test_0_interface_array_cast' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_interface_array_cast().ToString() + "'");
            }
            if (Tests.test_0_regress_74549() != 0)
            {
                Console.WriteLine("Test 'test_0_regress_74549' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_regress_74549().ToString() + "'");
            }
            if (Tests.test_0_regress_75832() != 0)
            {
                Console.WriteLine("Test 'test_0_regress_75832' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_regress_75832().ToString() + "'");
            }
            if (Tests.test_0_stelem_ref_null_opt() != 0)
            {
                Console.WriteLine("Test 'test_0_stelem_ref_null_opt' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_stelem_ref_null_opt().ToString() + "'");
            }
            if (Tests.test_0_invalid_new_array_size() != 0)
            {
                Console.WriteLine("Test 'test_0_invalid_new_array_size' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_invalid_new_array_size().ToString() + "'");
            }
            if (Tests.test_1234_checked_i4_cast() != 1234)
            {
                Console.WriteLine("Test 'test_1234_checked_i4_cast' didn't return expected value. Expected: '1234' Got: '" + Tests.test_1234_checked_i4_cast().ToString() + "'");
            }
            if (Tests.test_10_int_uint_compare() != 10)
            {
                Console.WriteLine("Test 'test_10_int_uint_compare' didn't return expected value. Expected: '10' Got: '" + Tests.test_10_int_uint_compare().ToString() + "'");
            }
            if (Tests.test_0_ulong_regress() != 0)
            {
                Console.WriteLine("Test 'test_0_ulong_regress' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_ulong_regress().ToString() + "'");
            }
            if (Tests.test_0_ulong_regress2() != 0)
            {
                Console.WriteLine("Test 'test_0_ulong_regress2' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_ulong_regress2().ToString() + "'");
            }
            if (Tests.test_0_assemble_long() != 0)
            {
                Console.WriteLine("Test 'test_0_assemble_long' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_assemble_long().ToString() + "'");
            }
            if (Tests.test_0_hash() != 0)
            {
                Console.WriteLine("Test 'test_0_hash' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_hash().ToString() + "'");
            }
            if (Tests.test_0_shift_regress() != 0)
            {
                Console.WriteLine("Test 'test_0_shift_regress' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_shift_regress().ToString() + "'");
            }
            if (Tests.test_1234_conv_ovf_u8() != 1234)
            {
                Console.WriteLine("Test 'test_1234_conv_ovf_u8' didn't return expected value. Expected: '1234' Got: '" + Tests.test_1234_conv_ovf_u8().ToString() + "'");
            }
            if (Tests.test_0_regress_cprop_80738() != 0)
            {
                Console.WriteLine("Test 'test_0_regress_cprop_80738' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_regress_cprop_80738().ToString() + "'");
            }
            if (Tests.test_0_conv_u() != 0)
            {
                Console.WriteLine("Test 'test_0_conv_u' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_conv_u().ToString() + "'");
            }
            if (Tests.test_0_lconv_to_u2() != 0)
            {
                Console.WriteLine("Test 'test_0_lconv_to_u2' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_lconv_to_u2().ToString() + "'");
            }
            if (Tests.test_0_beq() != 0)
            {
                Console.WriteLine("Test 'test_0_beq' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_beq().ToString() + "'");
            }
            if (Tests.test_0_bne_un() != 0)
            {
                Console.WriteLine("Test 'test_0_bne_un' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_bne_un().ToString() + "'");
            }
            if (Tests.test_0_conv_r8() != 0)
            {
                Console.WriteLine("Test 'test_0_conv_r8' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_conv_r8().ToString() + "'");
            }
            if (Tests.test_0_conv_i() != 0)
            {
                Console.WriteLine("Test 'test_0_conv_i' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_conv_i().ToString() + "'");
            }
            if (Tests.test_5_conv_r4() != 5)
            {
                Console.WriteLine("Test 'test_5_conv_r4' didn't return expected value. Expected: '5' Got: '" + Tests.test_5_conv_r4().ToString() + "'");
            }
            if (Tests.test_0_conv_r4_m1() != 0)
            {
                Console.WriteLine("Test 'test_0_conv_r4_m1' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_conv_r4_m1().ToString() + "'");
            }
            if (Tests.test_5_double_conv_r4() != 5)
            {
                Console.WriteLine("Test 'test_5_double_conv_r4' didn't return expected value. Expected: '5' Got: '" + Tests.test_5_double_conv_r4().ToString() + "'");
            }
            if (Tests.test_5_float_conv_r8() != 5)
            {
                Console.WriteLine("Test 'test_5_float_conv_r8' didn't return expected value. Expected: '5' Got: '" + Tests.test_5_float_conv_r8().ToString() + "'");
            }
            if (Tests.test_5_conv_r8() != 5)
            {
                Console.WriteLine("Test 'test_5_conv_r8' didn't return expected value. Expected: '5' Got: '" + Tests.test_5_conv_r8().ToString() + "'");
            }
            if (Tests.test_5_add() != 5)
            {
                Console.WriteLine("Test 'test_5_add' didn't return expected value. Expected: '5' Got: '" + Tests.test_5_add().ToString() + "'");
            }
            if (Tests.test_5_sub() != 5)
            {
                Console.WriteLine("Test 'test_5_sub' didn't return expected value. Expected: '5' Got: '" + Tests.test_5_sub().ToString() + "'");
            }
            if (Tests.test_24_mul() != 24)
            {
                Console.WriteLine("Test 'test_24_mul' didn't return expected value. Expected: '24' Got: '" + Tests.test_24_mul().ToString() + "'");
            }
            if (Tests.test_4_div() != 4)
            {
                Console.WriteLine("Test 'test_4_div' didn't return expected value. Expected: '4' Got: '" + Tests.test_4_div().ToString() + "'");
            }
            if (Tests.test_2_rem() != 2)
            {
                Console.WriteLine("Test 'test_2_rem' didn't return expected value. Expected: '2' Got: '" + Tests.test_2_rem().ToString() + "'");
            }
            if (Tests.test_2_neg() != 2)
            {
                Console.WriteLine("Test 'test_2_neg' didn't return expected value. Expected: '2' Got: '" + Tests.test_2_neg().ToString() + "'");
            }
            if (Tests.test_46_float_add_spill() != 46)
            {
                Console.WriteLine("Test 'test_46_float_add_spill' didn't return expected value. Expected: '46' Got: '" + Tests.test_46_float_add_spill().ToString() + "'");
            }
            if (Tests.test_4_float_sub_spill() != 4)
            {
                Console.WriteLine("Test 'test_4_float_sub_spill' didn't return expected value. Expected: '4' Got: '" + Tests.test_4_float_sub_spill().ToString() + "'");
            }
            if (Tests.test_362880_float_mul_spill() != 362880)
            {
                Console.WriteLine("Test 'test_362880_float_mul_spill' didn't return expected value. Expected: '362880' Got: '" + Tests.test_362880_float_mul_spill().ToString() + "'");
            }
            if (Tests.test_4_long_cast() != 4)
            {
                Console.WriteLine("Test 'test_4_long_cast' didn't return expected value. Expected: '4' Got: '" + Tests.test_4_long_cast().ToString() + "'");
            }
            if (Tests.test_4_ulong_cast() != 4)
            {
                Console.WriteLine("Test 'test_4_ulong_cast' didn't return expected value. Expected: '4' Got: '" + Tests.test_4_ulong_cast().ToString() + "'");
            }
            if (Tests.test_4_single_long_cast() != 4)
            {
                Console.WriteLine("Test 'test_4_single_long_cast' didn't return expected value. Expected: '4' Got: '" + Tests.test_4_single_long_cast().ToString() + "'");
            }
            if (Tests.test_0_lconv_to_r8() != 0)
            {
                Console.WriteLine("Test 'test_0_lconv_to_r8' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_lconv_to_r8().ToString() + "'");
            }
            if (Tests.test_0_lconv_to_r4() != 0)
            {
                Console.WriteLine("Test 'test_0_lconv_to_r4' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_lconv_to_r4().ToString() + "'");
            }
            if (Tests.test_0_ftol_clobber_Float() != 0)
            {
                Console.WriteLine("Test 'test_0_ftol_clobber_Float' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_ftol_clobber_Float().ToString() + "'");
            }
            if (Tests.test_0_rounding() != 0)
            {
                Console.WriteLine("Test 'test_0_rounding' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_rounding().ToString() + "'");
            }
            if (Tests.test_16_float_cmp() != 16)
            {
                Console.WriteLine("Test 'test_16_float_cmp' didn't return expected value. Expected: '16' Got: '" + Tests.test_16_float_cmp().ToString() + "'");
            }
            if (Tests.test_15_float_cmp_un() != 15)
            {
                Console.WriteLine("Test 'test_15_float_cmp_un' didn't return expected value. Expected: '15' Got: '" + Tests.test_15_float_cmp_un().ToString() + "'");
            }
            if (Tests.test_15_float_branch() != 15)
            {
                Console.WriteLine("Test 'test_15_float_branch' didn't return expected value. Expected: '15' Got: '" + Tests.test_15_float_branch().ToString() + "'");
            }
            if (Tests.test_15_float_branch_un() != 15)
            {
                Console.WriteLine("Test 'test_15_float_branch_un' didn't return expected value. Expected: '15' Got: '" + Tests.test_15_float_branch_un().ToString() + "'");
            }
            if (Tests.test_0_float_precision() != 0)
            {
                Console.WriteLine("Test 'test_0_float_precision' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_float_precision().ToString() + "'");
            }
            if (Tests.test_15_clobber_1() != 15)
            {
                Console.WriteLine("Test 'test_15_clobber_1' didn't return expected value. Expected: '15' Got: '" + Tests.test_15_clobber_1().ToString() + "'");
            }
            if (Tests.test_15_clobber_1_fp() != 15)
            {
                Console.WriteLine("Test 'test_15_clobber_1_fp' didn't return expected value. Expected: '15' Got: '" + Tests.test_15_clobber_1_fp().ToString() + "'");
            }
            if (Tests.test_5_call_clobber() != 5)
            {
                Console.WriteLine("Test 'test_5_call_clobber' didn't return expected value. Expected: '5' Got: '" + Tests.test_5_call_clobber().ToString() + "'");
            }
            if (Tests.test_7_call_clobber_dreg() != 7)
            {
                Console.WriteLine("Test 'test_7_call_clobber_dreg' didn't return expected value. Expected: '7' Got: '" + Tests.test_7_call_clobber_dreg().ToString() + "'");
            }
            if (Tests.test_9_spill_if_then_else() != 9)
            {
                Console.WriteLine("Test 'test_9_spill_if_then_else' didn't return expected value. Expected: '9' Got: '" + Tests.test_9_spill_if_then_else().ToString() + "'");
            }
            if (Tests.test_3_spill_reload_if_then_else() != 3)
            {
                Console.WriteLine("Test 'test_3_spill_reload_if_then_else' didn't return expected value. Expected: '3' Got: '" + Tests.test_3_spill_reload_if_then_else().ToString() + "'");
            }
            if (Tests.test_5_spill_loop() != 5)
            {
                Console.WriteLine("Test 'test_5_spill_loop' didn't return expected value. Expected: '5' Got: '" + Tests.test_5_spill_loop().ToString() + "'");
            }
            if (Tests.test_0_volatile() != 0)
            {
                Console.WriteLine("Test 'test_0_volatile' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_volatile().ToString() + "'");
            }
            if (Tests.test_0_volatile_unused() != 0)
            {
                Console.WriteLine("Test 'test_0_volatile_unused' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_volatile_unused().ToString() + "'");
            }
            if (Tests.test_0_volatile_unused_2() != 0)
            {
                Console.WriteLine("Test 'test_0_volatile_unused_2' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_volatile_unused_2().ToString() + "'");
            }
            if (Tests.test_0_volatile_unused_3() != 0)
            {
                Console.WriteLine("Test 'test_0_volatile_unused_3' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_volatile_unused_3().ToString() + "'");
            }
            if (Tests.test_0_volatile_regress_1() != 0)
            {
                Console.WriteLine("Test 'test_0_volatile_regress_1' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_volatile_regress_1().ToString() + "'");
            }
            if (Tests.test_29_volatile_regress_2() != 29)
            {
                Console.WriteLine("Test 'test_29_volatile_regress_2' didn't return expected value. Expected: '29' Got: '" + Tests.test_29_volatile_regress_2().ToString() + "'");
            }
            if (Tests.test_0_clobber_regress_1() != 0)
            {
                Console.WriteLine("Test 'test_0_clobber_regress_1' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_clobber_regress_1().ToString() + "'");
            }
            if (Tests.test_0_spill_regress_1() != 0)
            {
                Console.WriteLine("Test 'test_0_spill_regress_1' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_spill_regress_1().ToString() + "'");
            }
            if (Tests.test_0_spill_regress_2() != 0)
            {
                Console.WriteLine("Test 'test_0_spill_regress_2' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_spill_regress_2().ToString() + "'");
            }
            if (Tests.test_0_unused_args() != 0)
            {
                Console.WriteLine("Test 'test_0_unused_args' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_unused_args().ToString() + "'");
            }
            if (Tests.test_0_spill_regress_3() != 0)
            {
                Console.WriteLine("Test 'test_0_spill_regress_3' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_spill_regress_3().ToString() + "'");
            }
            if (Tests.test_0_spill_regress_4() != 0)
            {
                Console.WriteLine("Test 'test_0_spill_regress_4' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_spill_regress_4().ToString() + "'");
            }
            if (Tests.test_0_do_while_critical_edges() != 0)
            {
                Console.WriteLine("Test 'test_0_do_while_critical_edges' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_do_while_critical_edges().ToString() + "'");
            }
            if (Tests.test_0_switch_critical_edges() != 0)
            {
                Console.WriteLine("Test 'test_0_switch_critical_edges' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_switch_critical_edges().ToString() + "'");
            }
            if (Tests.test_0_sin_precision() != 0)
            {
                Console.WriteLine("Test 'test_0_sin_precision' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_sin_precision().ToString() + "'");
            }
            if (Tests.test_0_cos_precision() != 0)
            {
                Console.WriteLine("Test 'test_0_cos_precision' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_cos_precision().ToString() + "'");
            }
            if (Tests.test_0_tan_precision() != 0)
            {
                Console.WriteLine("Test 'test_0_tan_precision' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_tan_precision().ToString() + "'");
            }
            if (Tests.test_0_atan_precision() != 0)
            {
                Console.WriteLine("Test 'test_0_atan_precision' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_atan_precision().ToString() + "'");
            }
            if (Tests.test_0_sqrt_precision() != 0)
            {
                Console.WriteLine("Test 'test_0_sqrt_precision' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_sqrt_precision().ToString() + "'");
            }
            if (Tests.test_2_sqrt() != 2)
            {
                Console.WriteLine("Test 'test_2_sqrt' didn't return expected value. Expected: '2' Got: '" + Tests.test_2_sqrt().ToString() + "'");
            }
            if (Tests.test_0_sqrt_precision_and_not_spill() != 0)
            {
                Console.WriteLine("Test 'test_0_sqrt_precision_and_not_spill' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_sqrt_precision_and_not_spill().ToString() + "'");
            }
            if (Tests.test_0_regress_668095_synchronized_gshared() != 0)
            {
                Console.WriteLine("Test 'test_0_regress_668095_synchronized_gshared' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_regress_668095_synchronized_gshared().ToString() + "'");
            }
            if (Tests.test_10_simple_cast() != 10)
            {
                Console.WriteLine("Test 'test_10_simple_cast' didn't return expected value. Expected: '10' Got: '" + Tests.test_10_simple_cast().ToString() + "'");
            }
            if (Tests.test_1_bigmul1() != 1)
            {
                Console.WriteLine("Test 'test_1_bigmul1' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_bigmul1().ToString() + "'");
            }
            if (Tests.test_1_bigmul2() != 1)
            {
                Console.WriteLine("Test 'test_1_bigmul2' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_bigmul2().ToString() + "'");
            }
            if (Tests.test_1_bigmul3() != 1)
            {
                Console.WriteLine("Test 'test_1_bigmul3' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_bigmul3().ToString() + "'");
            }
            if (Tests.test_1_bigmul4() != 1)
            {
                Console.WriteLine("Test 'test_1_bigmul4' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_bigmul4().ToString() + "'");
            }
            if (Tests.test_1_bigmul5() != 1)
            {
                Console.WriteLine("Test 'test_1_bigmul5' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_bigmul5().ToString() + "'");
            }
            if (Tests.test_1_bigmul6() != 1)
            {
                Console.WriteLine("Test 'test_1_bigmul6' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_bigmul6().ToString() + "'");
            }
            if (Tests.test_0_beq_Long() != 0)
            {
                Console.WriteLine("Test 'test_0_beq_Long' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_beq_Long().ToString() + "'");
            }
            if (Tests.test_0_bne_un_Long() != 0)
            {
                Console.WriteLine("Test 'test_0_bne_un_Long' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_bne_un_Long().ToString() + "'");
            }
            if (Tests.test_0_ble() != 0)
            {
                Console.WriteLine("Test 'test_0_ble' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_ble().ToString() + "'");
            }
            if (Tests.test_0_ble_un() != 0)
            {
                Console.WriteLine("Test 'test_0_ble_un' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_ble_un().ToString() + "'");
            }
            if (Tests.test_0_bge() != 0)
            {
                Console.WriteLine("Test 'test_0_bge' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_bge().ToString() + "'");
            }
            if (Tests.test_0_bge_un() != 0)
            {
                Console.WriteLine("Test 'test_0_bge_un' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_bge_un().ToString() + "'");
            }
            if (Tests.test_0_blt() != 0)
            {
                Console.WriteLine("Test 'test_0_blt' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_blt().ToString() + "'");
            }
            if (Tests.test_0_blt_un() != 0)
            {
                Console.WriteLine("Test 'test_0_blt_un' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_blt_un().ToString() + "'");
            }
            if (Tests.test_0_bgt() != 0)
            {
                Console.WriteLine("Test 'test_0_bgt' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_bgt().ToString() + "'");
            }
            if (Tests.test_0_bgt_un() != 0)
            {
                Console.WriteLine("Test 'test_0_bgt_un' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_bgt_un().ToString() + "'");
            }
            if (Tests.test_0_conv_to_i4() != 0)
            {
                Console.WriteLine("Test 'test_0_conv_to_i4' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_conv_to_i4().ToString() + "'");
            }
            if (Tests.test_32_conv_to_u4() != 32)
            {
                Console.WriteLine("Test 'test_32_conv_to_u4' didn't return expected value. Expected: '32' Got: '" + Tests.test_32_conv_to_u4().ToString() + "'");
            }
            if (Tests.test_15_conv_to_u4_2() != 15)
            {
                Console.WriteLine("Test 'test_15_conv_to_u4_2' didn't return expected value. Expected: '15' Got: '" + Tests.test_15_conv_to_u4_2().ToString() + "'");
            }
            if (Tests.test_0_conv_from_i4() != 0)
            {
                Console.WriteLine("Test 'test_0_conv_from_i4' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_conv_from_i4().ToString() + "'");
            }
            if (Tests.test_0_conv_from_i4_negative() != 0)
            {
                Console.WriteLine("Test 'test_0_conv_from_i4_negative' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_conv_from_i4_negative().ToString() + "'");
            }
            if (Tests.test_8_and() != 8)
            {
                Console.WriteLine("Test 'test_8_and' didn't return expected value. Expected: '8' Got: '" + Tests.test_8_and().ToString() + "'");
            }
            if (Tests.test_8_and_imm() != 8)
            {
                Console.WriteLine("Test 'test_8_and_imm' didn't return expected value. Expected: '8' Got: '" + Tests.test_8_and_imm().ToString() + "'");
            }
            if (Tests.test_1_and() != 1)
            {
                Console.WriteLine("Test 'test_1_and' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_and().ToString() + "'");
            }
            if (Tests.test_10_or() != 10)
            {
                Console.WriteLine("Test 'test_10_or' didn't return expected value. Expected: '10' Got: '" + Tests.test_10_or().ToString() + "'");
            }
            if (Tests.test_10_or_imm() != 10)
            {
                Console.WriteLine("Test 'test_10_or_imm' didn't return expected value. Expected: '10' Got: '" + Tests.test_10_or_imm().ToString() + "'");
            }
            if (Tests.test_5_xor() != 5)
            {
                Console.WriteLine("Test 'test_5_xor' didn't return expected value. Expected: '5' Got: '" + Tests.test_5_xor().ToString() + "'");
            }
            if (Tests.test_5_xor_imm() != 5)
            {
                Console.WriteLine("Test 'test_5_xor_imm' didn't return expected value. Expected: '5' Got: '" + Tests.test_5_xor_imm().ToString() + "'");
            }
            if (Tests.test_5_add_Long() != 5)
            {
                Console.WriteLine("Test 'test_5_add_Long' didn't return expected value. Expected: '5' Got: '" + Tests.test_5_add_Long().ToString() + "'");
            }
            if (Tests.test_5_add_imm() != 5)
            {
                Console.WriteLine("Test 'test_5_add_imm' didn't return expected value. Expected: '5' Got: '" + Tests.test_5_add_imm().ToString() + "'");
            }
            if (Tests.test_0_add_imm_carry() != 0)
            {
                Console.WriteLine("Test 'test_0_add_imm_carry' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_add_imm_carry().ToString() + "'");
            }
            if (Tests.test_0_add_imm_no_inc() != 0)
            {
                Console.WriteLine("Test 'test_0_add_imm_no_inc' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_add_imm_no_inc().ToString() + "'");
            }
            if (Tests.test_4_addcc_imm() != 4)
            {
                Console.WriteLine("Test 'test_4_addcc_imm' didn't return expected value. Expected: '4' Got: '" + Tests.test_4_addcc_imm().ToString() + "'");
            }
            if (Tests.test_5_sub_Long() != 5)
            {
                Console.WriteLine("Test 'test_5_sub_Long' didn't return expected value. Expected: '5' Got: '" + Tests.test_5_sub_Long().ToString() + "'");
            }
            if (Tests.test_5_sub_imm() != 5)
            {
                Console.WriteLine("Test 'test_5_sub_imm' didn't return expected value. Expected: '5' Got: '" + Tests.test_5_sub_imm().ToString() + "'");
            }
            if (Tests.test_0_sub_imm_carry() != 0)
            {
                Console.WriteLine("Test 'test_0_sub_imm_carry' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_sub_imm_carry().ToString() + "'");
            }
            if (Tests.test_0_add_ovf() != 0)
            {
                Console.WriteLine("Test 'test_0_add_ovf' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_add_ovf().ToString() + "'");
            }
            if (Tests.test_0_add_un_ovf() != 0)
            {
                Console.WriteLine("Test 'test_0_add_un_ovf' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_add_un_ovf().ToString() + "'");
            }
            if (Tests.test_0_sub_ovf() != 0)
            {
                Console.WriteLine("Test 'test_0_sub_ovf' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_sub_ovf().ToString() + "'");
            }
            if (Tests.test_0_sub_ovf_un() != 0)
            {
                Console.WriteLine("Test 'test_0_sub_ovf_un' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_sub_ovf_un().ToString() + "'");
            }
            if (Tests.test_2_neg_Long() != 2)
            {
                Console.WriteLine("Test 'test_2_neg_Long' didn't return expected value. Expected: '2' Got: '" + Tests.test_2_neg_Long().ToString() + "'");
            }
            if (Tests.test_0_neg_large() != 0)
            {
                Console.WriteLine("Test 'test_0_neg_large' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_neg_large().ToString() + "'");
            }
            if (Tests.test_5_shift() != 5)
            {
                Console.WriteLine("Test 'test_5_shift' didn't return expected value. Expected: '5' Got: '" + Tests.test_5_shift().ToString() + "'");
            }
            if (Tests.test_1_shift_u() != 1)
            {
                Console.WriteLine("Test 'test_1_shift_u' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_shift_u().ToString() + "'");
            }
            if (Tests.test_1_shift_u_32() != 1)
            {
                Console.WriteLine("Test 'test_1_shift_u_32' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_shift_u_32().ToString() + "'");
            }
            if (Tests.test_1_simple_neg() != 1)
            {
                Console.WriteLine("Test 'test_1_simple_neg' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_simple_neg().ToString() + "'");
            }
            if (Tests.test_2_compare() != 2)
            {
                Console.WriteLine("Test 'test_2_compare' didn't return expected value. Expected: '2' Got: '" + Tests.test_2_compare().ToString() + "'");
            }
            if (Tests.test_9_alu() != 9)
            {
                Console.WriteLine("Test 'test_9_alu' didn't return expected value. Expected: '9' Got: '" + Tests.test_9_alu().ToString() + "'");
            }
            if (Tests.test_24_mul_Long() != 24)
            {
                Console.WriteLine("Test 'test_24_mul_Long' didn't return expected value. Expected: '24' Got: '" + Tests.test_24_mul_Long().ToString() + "'");
            }
            if (Tests.test_24_mul_ovf() != 24)
            {
                Console.WriteLine("Test 'test_24_mul_ovf' didn't return expected value. Expected: '24' Got: '" + Tests.test_24_mul_ovf().ToString() + "'");
            }
            if (Tests.test_24_mul_un() != 24)
            {
                Console.WriteLine("Test 'test_24_mul_un' didn't return expected value. Expected: '24' Got: '" + Tests.test_24_mul_un().ToString() + "'");
            }
            if (Tests.test_24_mul_ovf_un() != 24)
            {
                Console.WriteLine("Test 'test_24_mul_ovf_un' didn't return expected value. Expected: '24' Got: '" + Tests.test_24_mul_ovf_un().ToString() + "'");
            }
            if (Tests.test_0_mul_imm() != 0)
            {
                Console.WriteLine("Test 'test_0_mul_imm' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_mul_imm().ToString() + "'");
            }
            if (Tests.test_0_mul_imm_opt() != 0)
            {
                Console.WriteLine("Test 'test_0_mul_imm_opt' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_mul_imm_opt().ToString() + "'");
            }
            if (Tests.test_4_divun() != 4)
            {
                Console.WriteLine("Test 'test_4_divun' didn't return expected value. Expected: '4' Got: '" + Tests.test_4_divun().ToString() + "'");
            }
            if (Tests.test_1431655764_bigdivun_imm() != 1431655764)
            {
                Console.WriteLine("Test 'test_1431655764_bigdivun_imm' didn't return expected value. Expected: '1431655764' Got: '" + Tests.test_1431655764_bigdivun_imm().ToString() + "'");
            }
            if (Tests.test_1431655764_bigdivun() != 1431655764)
            {
                Console.WriteLine("Test 'test_1431655764_bigdivun' didn't return expected value. Expected: '1431655764' Got: '" + Tests.test_1431655764_bigdivun().ToString() + "'");
            }
            if (Tests.test_1_remun() != 1)
            {
                Console.WriteLine("Test 'test_1_remun' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_remun().ToString() + "'");
            }
            if (Tests.test_2_bigremun() != 2)
            {
                Console.WriteLine("Test 'test_2_bigremun' didn't return expected value. Expected: '2' Got: '" + Tests.test_2_bigremun().ToString() + "'");
            }
            if (Tests.test_0_ceq() != 0)
            {
                Console.WriteLine("Test 'test_0_ceq' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_ceq().ToString() + "'");
            }
            if (Tests.test_0_ceq_complex() != 0)
            {
                Console.WriteLine("Test 'test_0_ceq_complex' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_ceq_complex().ToString() + "'");
            }
            if (Tests.test_0_clt() != 0)
            {
                Console.WriteLine("Test 'test_0_clt' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_clt().ToString() + "'");
            }
            if (Tests.test_0_clt_un() != 0)
            {
                Console.WriteLine("Test 'test_0_clt_un' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_clt_un().ToString() + "'");
            }
            if (Tests.test_0_cgt() != 0)
            {
                Console.WriteLine("Test 'test_0_cgt' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_cgt().ToString() + "'");
            }
            if (Tests.test_0_cgt_un() != 0)
            {
                Console.WriteLine("Test 'test_0_cgt_un' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_cgt_un().ToString() + "'");
            }
            if (Tests.test_3_byte_cast() != 3)
            {
                Console.WriteLine("Test 'test_3_byte_cast' didn't return expected value. Expected: '3' Got: '" + Tests.test_3_byte_cast().ToString() + "'");
            }
            if (Tests.test_4_ushort_cast() != 4)
            {
                Console.WriteLine("Test 'test_4_ushort_cast' didn't return expected value. Expected: '4' Got: '" + Tests.test_4_ushort_cast().ToString() + "'");
            }
            if (Tests.test_500_mul_div() != 500)
            {
                Console.WriteLine("Test 'test_500_mul_div' didn't return expected value. Expected: '500' Got: '" + Tests.test_500_mul_div().ToString() + "'");
            }
            if (Tests.test_3_checked_cast_un() != 3)
            {
                Console.WriteLine("Test 'test_3_checked_cast_un' didn't return expected value. Expected: '3' Got: '" + Tests.test_3_checked_cast_un().ToString() + "'");
            }
            if (Tests.test_4_checked_cast() != 4)
            {
                Console.WriteLine("Test 'test_4_checked_cast' didn't return expected value. Expected: '4' Got: '" + Tests.test_4_checked_cast().ToString() + "'");
            }
            if (Tests.test_12_checked_i1_cast() != 12)
            {
                Console.WriteLine("Test 'test_12_checked_i1_cast' didn't return expected value. Expected: '12' Got: '" + Tests.test_12_checked_i1_cast().ToString() + "'");
            }
            if (Tests.test_127_checked_i1_cast_un() != 127)
            {
                Console.WriteLine("Test 'test_127_checked_i1_cast_un' didn't return expected value. Expected: '127' Got: '" + Tests.test_127_checked_i1_cast_un().ToString() + "'");
            }
            if (Tests.test_1234_checked_i2_cast() != 1234)
            {
                Console.WriteLine("Test 'test_1234_checked_i2_cast' didn't return expected value. Expected: '1234' Got: '" + Tests.test_1234_checked_i2_cast().ToString() + "'");
            }
            if (Tests.test_32767_checked_i2_cast_un() != 32767)
            {
                Console.WriteLine("Test 'test_32767_checked_i2_cast_un' didn't return expected value. Expected: '32767' Got: '" + Tests.test_32767_checked_i2_cast_un().ToString() + "'");
            }
            if (Tests.test_0_div_opt() != 0)
            {
                Console.WriteLine("Test 'test_0_div_opt' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_div_opt().ToString() + "'");
            }
            if (Tests.test_0_rem_opt() != 0)
            {
                Console.WriteLine("Test 'test_0_rem_opt' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_rem_opt().ToString() + "'");
            }
            if (Tests.test_0_branch_to_cmov_opt() != 0)
            {
                Console.WriteLine("Test 'test_0_branch_to_cmov_opt' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_branch_to_cmov_opt().ToString() + "'");
            }
            if (Tests.test_0_ishr_sign_extend() != 0)
            {
                Console.WriteLine("Test 'test_0_ishr_sign_extend' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_ishr_sign_extend().ToString() + "'");
            }
            if (Tests.test_0_ishr_sign_extend_cfold() != 0)
            {
                Console.WriteLine("Test 'test_0_ishr_sign_extend_cfold' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_ishr_sign_extend_cfold().ToString() + "'");
            }
            if (Tests.test_1_nullable_unbox() != 1)
            {
                Console.WriteLine("Test 'test_1_nullable_unbox' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_nullable_unbox().ToString() + "'");
            }
            if (Tests.test_1_nullable_unbox_null() != 1)
            {
                Console.WriteLine("Test 'test_1_nullable_unbox_null' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_nullable_unbox_null().ToString() + "'");
            }
            if (Tests.test_1_nullable_box() != 1)
            {
                Console.WriteLine("Test 'test_1_nullable_box' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_nullable_box().ToString() + "'");
            }
            if (Tests.test_1_nullable_box_null() != 1)
            {
                Console.WriteLine("Test 'test_1_nullable_box_null' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_nullable_box_null().ToString() + "'");
            }
            if (Tests.test_1_isinst_nullable() != 1)
            {
                Console.WriteLine("Test 'test_1_isinst_nullable' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_isinst_nullable().ToString() + "'");
            }
            if (Tests.test_1_nullable_unbox_vtype() != 1)
            {
                Console.WriteLine("Test 'test_1_nullable_unbox_vtype' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_nullable_unbox_vtype().ToString() + "'");
            }
            if (Tests.test_1_nullable_unbox_null_vtype() != 1)
            {
                Console.WriteLine("Test 'test_1_nullable_unbox_null_vtype' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_nullable_unbox_null_vtype().ToString() + "'");
            }
            if (Tests.test_1_nullable_box_vtype() != 1)
            {
                Console.WriteLine("Test 'test_1_nullable_box_vtype' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_nullable_box_vtype().ToString() + "'");
            }
            if (Tests.test_1_nullable_box_null_vtype() != 1)
            {
                Console.WriteLine("Test 'test_1_nullable_box_null_vtype' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_nullable_box_null_vtype().ToString() + "'");
            }
            if (Tests.test_1_isinst_nullable_vtype() != 1)
            {
                Console.WriteLine("Test 'test_1_isinst_nullable_vtype' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_isinst_nullable_vtype().ToString() + "'");
            }
            if (Tests.test_0_nullable_normal_unbox() != 0)
            {
                Console.WriteLine("Test 'test_0_nullable_normal_unbox' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_nullable_normal_unbox().ToString() + "'");
            }
            if (Tests.test_1_ldelem_stelem_any_int() != 1)
            {
                Console.WriteLine("Test 'test_1_ldelem_stelem_any_int' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_ldelem_stelem_any_int().ToString() + "'");
            }
            if (Tests.test_0_ldelema() != 0)
            {
                Console.WriteLine("Test 'test_0_ldelema' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_ldelema().ToString() + "'");
            }
            if (Tests.test_0_newarr_multi_dim() != 0)
            {
                Console.WriteLine("Test 'test_0_newarr_multi_dim' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_newarr_multi_dim().ToString() + "'");
            }
            if (Tests.test_0_iface_call_null_bug_77442() != 0)
            {
                Console.WriteLine("Test 'test_0_iface_call_null_bug_77442' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_iface_call_null_bug_77442().ToString() + "'");
            }
            if (Tests.test_18_ldobj_stobj_generics() != 18)
            {
                Console.WriteLine("Test 'test_18_ldobj_stobj_generics' didn't return expected value. Expected: '18' Got: '" + Tests.test_18_ldobj_stobj_generics().ToString() + "'");
            }
            if (Tests.test_5_ldelem_stelem_generics() != 5)
            {
                Console.WriteLine("Test 'test_5_ldelem_stelem_generics' didn't return expected value. Expected: '5' Got: '" + Tests.test_5_ldelem_stelem_generics().ToString() + "'");
            }
            if (Tests.test_0_constrained_vtype_box() != 0)
            {
                Console.WriteLine("Test 'test_0_constrained_vtype_box' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_constrained_vtype_box().ToString() + "'");
            }
            if (Tests.test_0_constrained_vtype() != 0)
            {
                Console.WriteLine("Test 'test_0_constrained_vtype' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_constrained_vtype().ToString() + "'");
            }
            if (Tests.test_0_constrained_reftype() != 0)
            {
                Console.WriteLine("Test 'test_0_constrained_reftype' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_constrained_reftype().ToString() + "'");
            }
            if (Tests.test_0_box_brtrue_optimizations() != 0)
            {
                Console.WriteLine("Test 'test_0_box_brtrue_optimizations' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_box_brtrue_optimizations().ToString() + "'");
            }
            if (Tests.test_0_generic_get_value_optimization_int() != 0)
            {
                Console.WriteLine("Test 'test_0_generic_get_value_optimization_int' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_generic_get_value_optimization_int().ToString() + "'");
            }
            if (Tests.test_0_generic_get_value_optimization_vtype() != 0)
            {
                Console.WriteLine("Test 'test_0_generic_get_value_optimization_vtype' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_generic_get_value_optimization_vtype().ToString() + "'");
            }
            if (Tests.test_0_nullable_ldflda() != 0)
            {
                Console.WriteLine("Test 'test_0_nullable_ldflda' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_nullable_ldflda().ToString() + "'");
            }
            if (Tests.test_0_ldfld_stfld_mro() != 0)
            {
                Console.WriteLine("Test 'test_0_ldfld_stfld_mro' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_ldfld_stfld_mro().ToString() + "'");
            }
            if (Tests.test_0_generic_virtual_call_on_vtype_unbox() != 0)
            {
                Console.WriteLine("Test 'test_0_generic_virtual_call_on_vtype_unbox' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_generic_virtual_call_on_vtype_unbox().ToString() + "'");
            }
            if (Tests.test_0_box_brtrue_opt() != 0)
            {
                Console.WriteLine("Test 'test_0_box_brtrue_opt' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_box_brtrue_opt().ToString() + "'");
            }
            if (Tests.test_0_box_brtrue_opt_regress_81102() != 0)
            {
                Console.WriteLine("Test 'test_0_box_brtrue_opt_regress_81102' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_box_brtrue_opt_regress_81102().ToString() + "'");
            }
            if (Tests.test_0_ldloca_initobj_opt() != 0)
            {
                Console.WriteLine("Test 'test_0_ldloca_initobj_opt' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_ldloca_initobj_opt().ToString() + "'");
            }
            if (Tests.test_0_ldvirtftn_generic_method() != 0)
            {
                Console.WriteLine("Test 'test_0_ldvirtftn_generic_method' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_ldvirtftn_generic_method().ToString() + "'");
            }
            if (Tests.test_0_throw_dead_this() != 0)
            {
                Console.WriteLine("Test 'test_0_throw_dead_this' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_throw_dead_this().ToString() + "'");
            }
            if (Tests.test_0_inline_infinite_polymorphic_recursion() != 0)
            {
                Console.WriteLine("Test 'test_0_inline_infinite_polymorphic_recursion' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_inline_infinite_polymorphic_recursion().ToString() + "'");
            }
            if (Tests.test_0_generic_virtual_on_interfaces() != 0)
            {
                Console.WriteLine("Test 'test_0_generic_virtual_on_interfaces' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_generic_virtual_on_interfaces().ToString() + "'");
            }
            if (Tests.test_0_generic_virtual_on_interfaces_ref() != 0)
            {
                Console.WriteLine("Test 'test_0_generic_virtual_on_interfaces_ref' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_generic_virtual_on_interfaces_ref().ToString() + "'");
            }
            if (Tests.test_2_cprop_bug() != 2)
            {
                Console.WriteLine("Test 'test_2_cprop_bug' didn't return expected value. Expected: '2' Got: '" + Tests.test_2_cprop_bug().ToString() + "'");
            }
            if (Tests.test_0_regress_550964_constrained_enum_long() != 0)
            {
                Console.WriteLine("Test 'test_0_regress_550964_constrained_enum_long' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_regress_550964_constrained_enum_long().ToString() + "'");
            }
            if (Tests.test_0_fullaot_linq() != 0)
            {
                Console.WriteLine("Test 'test_0_fullaot_linq' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_fullaot_linq().ToString() + "'");
            }
            if (Tests.test_0_fullaot_comparer_t() != 0)
            {
                Console.WriteLine("Test 'test_0_fullaot_comparer_t' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_fullaot_comparer_t().ToString() + "'");
            }
            if (Tests.test_0_fullaot_comparer_t_2() != 0)
            {
                Console.WriteLine("Test 'test_0_fullaot_comparer_t_2' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_fullaot_comparer_t_2().ToString() + "'");
            }
            if (Tests.test_0_fullaot_array_wrappers() != 0)
            {
                Console.WriteLine("Test 'test_0_fullaot_array_wrappers' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_fullaot_array_wrappers().ToString() + "'");
            }
            if (Tests.test_2_generic_class_init_gshared_ctor() != 2)
            {
                Console.WriteLine("Test 'test_2_generic_class_init_gshared_ctor' didn't return expected value. Expected: '2' Got: '" + Tests.test_2_generic_class_init_gshared_ctor().ToString() + "'");
            }
            if (Tests.test_2_generic_class_init_gshared_ctor_from_gshared() != 2)
            {
                Console.WriteLine("Test 'test_2_generic_class_init_gshared_ctor_from_gshared' didn't return expected value. Expected: '2' Got: '" + Tests.test_2_generic_class_init_gshared_ctor_from_gshared().ToString() + "'");
            }
            if (Tests.test_0_gshared_delegate_rgctx() != 0)
            {
                Console.WriteLine("Test 'test_0_gshared_delegate_rgctx' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_gshared_delegate_rgctx().ToString() + "'");
            }
            if (Tests.test_0_gshared_delegate_from_gshared() != 0)
            {
                Console.WriteLine("Test 'test_0_gshared_delegate_from_gshared' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_gshared_delegate_from_gshared().ToString() + "'");
            }
            if (Tests.test_0_marshalbyref_call_from_gshared_virt_elim() != 0)
            {
                Console.WriteLine("Test 'test_0_marshalbyref_call_from_gshared_virt_elim' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_marshalbyref_call_from_gshared_virt_elim().ToString() + "'");
            }
            if (Tests.test_0_bug_620864() != 0)
            {
                Console.WriteLine("Test 'test_0_bug_620864' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_bug_620864().ToString() + "'");
            }
            if (Tests.test_0_infinite_generic_recursion() != 0)
            {
                Console.WriteLine("Test 'test_0_infinite_generic_recursion' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_infinite_generic_recursion().ToString() + "'");
            }
            if (Tests.test_0_full_aot_nullable_unbox_from_gshared_code() != 0)
            {
                Console.WriteLine("Test 'test_0_full_aot_nullable_unbox_from_gshared_code' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_full_aot_nullable_unbox_from_gshared_code().ToString() + "'");
            }
            if (Tests.test_0_partial_sharing() != 0)
            {
                Console.WriteLine("Test 'test_0_partial_sharing' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_partial_sharing().ToString() + "'");
            }
            if (Tests.test_6_partial_sharing_linq() != 6)
            {
                Console.WriteLine("Test 'test_6_partial_sharing_linq' didn't return expected value. Expected: '6' Got: '" + Tests.test_6_partial_sharing_linq().ToString() + "'");
            }
            if (Tests.test_0_partial_shared_method_in_nonshared_class() != 0)
            {
                Console.WriteLine("Test 'test_0_partial_shared_method_in_nonshared_class' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_partial_shared_method_in_nonshared_class().ToString() + "'");
            }
            if (Tests.test_0_add_ovf2() != 0)
            {
                Console.WriteLine("Test 'test_0_add_ovf2' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_add_ovf2().ToString() + "'");
            }
            if (Tests.test_0_add_ovf3() != 0)
            {
                Console.WriteLine("Test 'test_0_add_ovf3' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_add_ovf3().ToString() + "'");
            }
            if (Tests.test_0_add_ovf4() != 0)
            {
                Console.WriteLine("Test 'test_0_add_ovf4' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_add_ovf4().ToString() + "'");
            }
            if (Tests.test_0_add_ovf5() != 0)
            {
                Console.WriteLine("Test 'test_0_add_ovf5' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_add_ovf5().ToString() + "'");
            }
            if (Tests.test_0_add_ovf6() != 0)
            {
                Console.WriteLine("Test 'test_0_add_ovf6' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_add_ovf6().ToString() + "'");
            }
            if (Tests.test_0_add_un_ovf_Basic() != 0)
            {
                Console.WriteLine("Test 'test_0_add_un_ovf_Basic' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_add_un_ovf_Basic().ToString() + "'");
            }
            if (Tests.test_0_sub_ovf1() != 0)
            {
                Console.WriteLine("Test 'test_0_sub_ovf1' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_sub_ovf1().ToString() + "'");
            }
            if (Tests.test_0_sub_ovf2() != 0)
            {
                Console.WriteLine("Test 'test_0_sub_ovf2' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_sub_ovf2().ToString() + "'");
            }
            if (Tests.test_0_sub_ovf3() != 0)
            {
                Console.WriteLine("Test 'test_0_sub_ovf3' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_sub_ovf3().ToString() + "'");
            }
            if (Tests.test_0_sub_ovf4() != 0)
            {
                Console.WriteLine("Test 'test_0_sub_ovf4' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_sub_ovf4().ToString() + "'");
            }
            if (Tests.test_0_sub_ovf5() != 0)
            {
                Console.WriteLine("Test 'test_0_sub_ovf5' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_sub_ovf5().ToString() + "'");
            }
            if (Tests.test_0_sub_ovf6() != 0)
            {
                Console.WriteLine("Test 'test_0_sub_ovf6' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_sub_ovf6().ToString() + "'");
            }
            if (Tests.test_0_sub_ovf_un_Basic() != 0)
            {
                Console.WriteLine("Test 'test_0_sub_ovf_un_Basic' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_sub_ovf_un_Basic().ToString() + "'");
            }
            if (Tests.test_3_or() != 3)
            {
                Console.WriteLine("Test 'test_3_or' didn't return expected value. Expected: '3' Got: '" + Tests.test_3_or().ToString() + "'");
            }
            if (Tests.test_3_or_un() != 3)
            {
                Console.WriteLine("Test 'test_3_or_un' didn't return expected value. Expected: '3' Got: '" + Tests.test_3_or_un().ToString() + "'");
            }
            if (Tests.test_3_or_short_un() != 3)
            {
                Console.WriteLine("Test 'test_3_or_short_un' didn't return expected value. Expected: '3' Got: '" + Tests.test_3_or_short_un().ToString() + "'");
            }
            if (Tests.test_18_or_imm() != 18)
            {
                Console.WriteLine("Test 'test_18_or_imm' didn't return expected value. Expected: '18' Got: '" + Tests.test_18_or_imm().ToString() + "'");
            }
            if (Tests.test_268435458_or_large_imm() != 268435458)
            {
                Console.WriteLine("Test 'test_268435458_or_large_imm' didn't return expected value. Expected: '268435458' Got: '" + Tests.test_268435458_or_large_imm().ToString() + "'");
            }
            if (Tests.test_268435459_or_large_imm2() != 268435459)
            {
                Console.WriteLine("Test 'test_268435459_or_large_imm2' didn't return expected value. Expected: '268435459' Got: '" + Tests.test_268435459_or_large_imm2().ToString() + "'");
            }
            if (Tests.test_1_xor() != 1)
            {
                Console.WriteLine("Test 'test_1_xor' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_xor().ToString() + "'");
            }
            if (Tests.test_1_xor_imm() != 1)
            {
                Console.WriteLine("Test 'test_1_xor_imm' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_xor_imm().ToString() + "'");
            }
            if (Tests.test_983041_xor_imm_large() != 983041)
            {
                Console.WriteLine("Test 'test_983041_xor_imm_large' didn't return expected value. Expected: '983041' Got: '" + Tests.test_983041_xor_imm_large().ToString() + "'");
            }
            if (Tests.test_1_neg() != 1)
            {
                Console.WriteLine("Test 'test_1_neg' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_neg().ToString() + "'");
            }
            if (Tests.test_2_not() != 2)
            {
                Console.WriteLine("Test 'test_2_not' didn't return expected value. Expected: '2' Got: '" + Tests.test_2_not().ToString() + "'");
            }
            if (Tests.test_16_shift() != 16)
            {
                Console.WriteLine("Test 'test_16_shift' didn't return expected value. Expected: '16' Got: '" + Tests.test_16_shift().ToString() + "'");
            }
            if (Tests.test_16_shift_add() != 16)
            {
                Console.WriteLine("Test 'test_16_shift_add' didn't return expected value. Expected: '16' Got: '" + Tests.test_16_shift_add().ToString() + "'");
            }
            if (Tests.test_16_shift_add2() != 16)
            {
                Console.WriteLine("Test 'test_16_shift_add2' didn't return expected value. Expected: '16' Got: '" + Tests.test_16_shift_add2().ToString() + "'");
            }
            if (Tests.test_16_shift_imm() != 16)
            {
                Console.WriteLine("Test 'test_16_shift_imm' didn't return expected value. Expected: '16' Got: '" + Tests.test_16_shift_imm().ToString() + "'");
            }
            if (Tests.test_524288_shift_imm_large() != 524288)
            {
                Console.WriteLine("Test 'test_524288_shift_imm_large' didn't return expected value. Expected: '524288' Got: '" + Tests.test_524288_shift_imm_large().ToString() + "'");
            }
            if (Tests.test_12_shift_imm_inv() != 12)
            {
                Console.WriteLine("Test 'test_12_shift_imm_inv' didn't return expected value. Expected: '12' Got: '" + Tests.test_12_shift_imm_inv().ToString() + "'");
            }
            if (Tests.test_12_shift_imm_inv_sbyte() != 12)
            {
                Console.WriteLine("Test 'test_12_shift_imm_inv_sbyte' didn't return expected value. Expected: '12' Got: '" + Tests.test_12_shift_imm_inv_sbyte().ToString() + "'");
            }
            if (Tests.test_1_rshift_imm() != 1)
            {
                Console.WriteLine("Test 'test_1_rshift_imm' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_rshift_imm().ToString() + "'");
            }
            if (Tests.test_2_unrshift_imm() != 2)
            {
                Console.WriteLine("Test 'test_2_unrshift_imm' didn't return expected value. Expected: '2' Got: '" + Tests.test_2_unrshift_imm().ToString() + "'");
            }
            if (Tests.test_0_bigunrshift_imm() != 0)
            {
                Console.WriteLine("Test 'test_0_bigunrshift_imm' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_bigunrshift_imm().ToString() + "'");
            }
            if (Tests.test_0_bigrshift_imm() != 0)
            {
                Console.WriteLine("Test 'test_0_bigrshift_imm' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_bigrshift_imm().ToString() + "'");
            }
            if (Tests.test_1_rshift() != 1)
            {
                Console.WriteLine("Test 'test_1_rshift' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_rshift().ToString() + "'");
            }
            if (Tests.test_2_unrshift() != 2)
            {
                Console.WriteLine("Test 'test_2_unrshift' didn't return expected value. Expected: '2' Got: '" + Tests.test_2_unrshift().ToString() + "'");
            }
            if (Tests.test_0_bigunrshift() != 0)
            {
                Console.WriteLine("Test 'test_0_bigunrshift' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_bigunrshift().ToString() + "'");
            }
            if (Tests.test_0_bigrshift() != 0)
            {
                Console.WriteLine("Test 'test_0_bigrshift' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_bigrshift().ToString() + "'");
            }
            if (Tests.test_2_cond() != 2)
            {
                Console.WriteLine("Test 'test_2_cond' didn't return expected value. Expected: '2' Got: '" + Tests.test_2_cond().ToString() + "'");
            }
            if (Tests.test_2_cond_short() != 2)
            {
                Console.WriteLine("Test 'test_2_cond_short' didn't return expected value. Expected: '2' Got: '" + Tests.test_2_cond_short().ToString() + "'");
            }
            if (Tests.test_2_cond_sbyte() != 2)
            {
                Console.WriteLine("Test 'test_2_cond_sbyte' didn't return expected value. Expected: '2' Got: '" + Tests.test_2_cond_sbyte().ToString() + "'");
            }
            if (Tests.test_6_cascade_cond() != 6)
            {
                Console.WriteLine("Test 'test_6_cascade_cond' didn't return expected value. Expected: '6' Got: '" + Tests.test_6_cascade_cond().ToString() + "'");
            }
            if (Tests.test_6_cascade_short() != 6)
            {
                Console.WriteLine("Test 'test_6_cascade_short' didn't return expected value. Expected: '6' Got: '" + Tests.test_6_cascade_short().ToString() + "'");
            }
            if (Tests.test_0_short_sign_extend() != 0)
            {
                Console.WriteLine("Test 'test_0_short_sign_extend' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_short_sign_extend().ToString() + "'");
            }
            if (Tests.test_127_iconv_to_i1() != 127)
            {
                Console.WriteLine("Test 'test_127_iconv_to_i1' didn't return expected value. Expected: '127' Got: '" + Tests.test_127_iconv_to_i1().ToString() + "'");
            }
            if (Tests.test_384_iconv_to_i2() != 384)
            {
                Console.WriteLine("Test 'test_384_iconv_to_i2' didn't return expected value. Expected: '384' Got: '" + Tests.test_384_iconv_to_i2().ToString() + "'");
            }
            if (Tests.test_15_for_loop() != 15)
            {
                Console.WriteLine("Test 'test_15_for_loop' didn't return expected value. Expected: '15' Got: '" + Tests.test_15_for_loop().ToString() + "'");
            }
            if (Tests.test_11_nested_for_loop() != 11)
            {
                Console.WriteLine("Test 'test_11_nested_for_loop' didn't return expected value. Expected: '11' Got: '" + Tests.test_11_nested_for_loop().ToString() + "'");
            }
            if (Tests.test_11_several_nested_for_loops() != 11)
            {
                Console.WriteLine("Test 'test_11_several_nested_for_loops' didn't return expected value. Expected: '11' Got: '" + Tests.test_11_several_nested_for_loops().ToString() + "'");
            }
            if (Tests.test_0_conv_ovf_i1() != 0)
            {
                Console.WriteLine("Test 'test_0_conv_ovf_i1' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_conv_ovf_i1().ToString() + "'");
            }
            if (Tests.test_0_conv_ovf_i1_un() != 0)
            {
                Console.WriteLine("Test 'test_0_conv_ovf_i1_un' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_conv_ovf_i1_un().ToString() + "'");
            }
            if (Tests.test_0_conv_ovf_i2() != 0)
            {
                Console.WriteLine("Test 'test_0_conv_ovf_i2' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_conv_ovf_i2().ToString() + "'");
            }
            if (Tests.test_0_conv_ovf_i2_un() != 0)
            {
                Console.WriteLine("Test 'test_0_conv_ovf_i2_un' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_conv_ovf_i2_un().ToString() + "'");
            }
            if (Tests.test_0_conv_ovf_u2() != 0)
            {
                Console.WriteLine("Test 'test_0_conv_ovf_u2' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_conv_ovf_u2().ToString() + "'");
            }
            if (Tests.test_0_conv_ovf_u2_un() != 0)
            {
                Console.WriteLine("Test 'test_0_conv_ovf_u2_un' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_conv_ovf_u2_un().ToString() + "'");
            }
            if (Tests.test_0_conv_ovf_u4() != 0)
            {
                Console.WriteLine("Test 'test_0_conv_ovf_u4' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_conv_ovf_u4().ToString() + "'");
            }
            if (Tests.test_0_conv_ovf_i4_un() != 0)
            {
                Console.WriteLine("Test 'test_0_conv_ovf_i4_un' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_conv_ovf_i4_un().ToString() + "'");
            }
            if (Tests.test_0_bool() != 0)
            {
                Console.WriteLine("Test 'test_0_bool' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_bool().ToString() + "'");
            }
            if (Tests.test_1_bool_inverted() != 1)
            {
                Console.WriteLine("Test 'test_1_bool_inverted' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_bool_inverted().ToString() + "'");
            }
            if (Tests.test_1_bool_assign() != 1)
            {
                Console.WriteLine("Test 'test_1_bool_assign' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_bool_assign().ToString() + "'");
            }
            if (Tests.test_1_bool_multi() != 1)
            {
                Console.WriteLine("Test 'test_1_bool_multi' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_bool_multi().ToString() + "'");
            }
            if (Tests.test_16_spill() != 16)
            {
                Console.WriteLine("Test 'test_16_spill' didn't return expected value. Expected: '16' Got: '" + Tests.test_16_spill().ToString() + "'");
            }
            if (Tests.test_1_switch() != 1)
            {
                Console.WriteLine("Test 'test_1_switch' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_switch().ToString() + "'");
            }
            if (Tests.test_0_switch_constprop() != 0)
            {
                Console.WriteLine("Test 'test_0_switch_constprop' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_switch_constprop().ToString() + "'");
            }
            if (Tests.test_0_switch_constprop2() != 0)
            {
                Console.WriteLine("Test 'test_0_switch_constprop2' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_switch_constprop2().ToString() + "'");
            }
            if (Tests.test_0_while_loop_1() != 0)
            {
                Console.WriteLine("Test 'test_0_while_loop_1' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_while_loop_1().ToString() + "'");
            }
            if (Tests.test_0_while_loop_2() != 0)
            {
                Console.WriteLine("Test 'test_0_while_loop_2' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_while_loop_2().ToString() + "'");
            }
            if (Tests.test_0_char_conv() != 0)
            {
                Console.WriteLine("Test 'test_0_char_conv' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_char_conv().ToString() + "'");
            }
            if (Tests.test_3_shift_regalloc() != 3)
            {
                Console.WriteLine("Test 'test_3_shift_regalloc' didn't return expected value. Expected: '3' Got: '" + Tests.test_3_shift_regalloc().ToString() + "'");
            }
            if (Tests.test_2_optimize_branches() != 2)
            {
                Console.WriteLine("Test 'test_2_optimize_branches' didn't return expected value. Expected: '2' Got: '" + Tests.test_2_optimize_branches().ToString() + "'");
            }
            if (Tests.test_0_checked_byte_cast() != 0)
            {
                Console.WriteLine("Test 'test_0_checked_byte_cast' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_checked_byte_cast().ToString() + "'");
            }
            if (Tests.test_0_checked_byte_cast_un() != 0)
            {
                Console.WriteLine("Test 'test_0_checked_byte_cast_un' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_checked_byte_cast_un().ToString() + "'");
            }
            if (Tests.test_0_checked_short_cast() != 0)
            {
                Console.WriteLine("Test 'test_0_checked_short_cast' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_checked_short_cast().ToString() + "'");
            }
            if (Tests.test_0_checked_short_cast_un() != 0)
            {
                Console.WriteLine("Test 'test_0_checked_short_cast_un' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_checked_short_cast_un().ToString() + "'");
            }
            if (Tests.test_1_a_eq_b_plus_a() != 1)
            {
                Console.WriteLine("Test 'test_1_a_eq_b_plus_a' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_a_eq_b_plus_a().ToString() + "'");
            }
            if (Tests.test_0_comp() != 0)
            {
                Console.WriteLine("Test 'test_0_comp' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_comp().ToString() + "'");
            }
            if (Tests.test_0_comp_unsigned() != 0)
            {
                Console.WriteLine("Test 'test_0_comp_unsigned' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_comp_unsigned().ToString() + "'");
            }
            if (Tests.test_16_cmov() != 16)
            {
                Console.WriteLine("Test 'test_16_cmov' didn't return expected value. Expected: '16' Got: '" + Tests.test_16_cmov().ToString() + "'");
            }
            if (Tests.test_0_and_cmp() != 0)
            {
                Console.WriteLine("Test 'test_0_and_cmp' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_and_cmp().ToString() + "'");
            }
            if (Tests.test_0_mul_imm_opt_Basic() != 0)
            {
                Console.WriteLine("Test 'test_0_mul_imm_opt_Basic' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_mul_imm_opt_Basic().ToString() + "'");
            }
            if (Tests.test_0_cne() != 0)
            {
                Console.WriteLine("Test 'test_0_cne' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_cne().ToString() + "'");
            }
            if (Tests.test_0_cmp_regvar_zero() != 0)
            {
                Console.WriteLine("Test 'test_0_cmp_regvar_zero' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_cmp_regvar_zero().ToString() + "'");
            }
            if (Tests.test_5_div_un_cfold() != 5)
            {
                Console.WriteLine("Test 'test_5_div_un_cfold' didn't return expected value. Expected: '5' Got: '" + Tests.test_5_div_un_cfold().ToString() + "'");
            }
            if (Tests.test_1_rem_un_cfold() != 1)
            {
                Console.WriteLine("Test 'test_1_rem_un_cfold' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_rem_un_cfold().ToString() + "'");
            }
            if (Tests.test_1_array_mismatch_2() != 1)
            {
                Console.WriteLine("Test 'test_1_array_mismatch_2' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_array_mismatch_2().ToString() + "'");
            }
            if (Tests.test_1_array_mismatch_3() != 1)
            {
                Console.WriteLine("Test 'test_1_array_mismatch_3' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_array_mismatch_3().ToString() + "'");
            }
            if (Tests.test_1_array_mismatch_4() != 1)
            {
                Console.WriteLine("Test 'test_1_array_mismatch_4' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_array_mismatch_4().ToString() + "'");
            }
            if (Tests.test_0_array_size() != 0)
            {
                Console.WriteLine("Test 'test_0_array_size' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_array_size().ToString() + "'");
            }
            if (Tests.test_0_stack_unwind() != 0)
            {
                Console.WriteLine("Test 'test_0_stack_unwind' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_stack_unwind().ToString() + "'");
            }
            if (Tests.test_0_throw_unwind() != 0)
            {
                Console.WriteLine("Test 'test_0_throw_unwind' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_throw_unwind().ToString() + "'");
            }
            if (Tests.test_0_regress_73242() != 0)
            {
                Console.WriteLine("Test 'test_0_regress_73242' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_regress_73242().ToString() + "'");
            }
            if (Tests.test_0_nullref() != 0)
            {
                Console.WriteLine("Test 'test_0_nullref' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_nullref().ToString() + "'");
            }
            if (Tests.test_0_nonvirt_nullref_at_clause_start() != 0)
            {
                Console.WriteLine("Test 'test_0_nonvirt_nullref_at_clause_start' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_nonvirt_nullref_at_clause_start().ToString() + "'");
            }
            if (Tests.test_0_inline_throw_only() != 0)
            {
                Console.WriteLine("Test 'test_0_inline_throw_only' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_inline_throw_only().ToString() + "'");
            }
            if (Tests.test_0_inline_throw_only_gettext() != 0)
            {
                Console.WriteLine("Test 'test_0_inline_throw_only_gettext' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_inline_throw_only_gettext().ToString() + "'");
            }
            if (Tests.test_0_throw_to_branch_opt_outer_clause() != 0)
            {
                Console.WriteLine("Test 'test_0_throw_to_branch_opt_outer_clause' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_throw_to_branch_opt_outer_clause().ToString() + "'");
            }
            if (Tests.test_0_try_inside_finally_cmov_opt() != 0)
            {
                Console.WriteLine("Test 'test_0_try_inside_finally_cmov_opt' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_try_inside_finally_cmov_opt().ToString() + "'");
            }
            if (Tests.test_0_inline_throw() != 0)
            {
                Console.WriteLine("Test 'test_0_inline_throw' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_inline_throw().ToString() + "'");
            }
            if (Tests.test_0_lmf_filter() != 0)
            {
                Console.WriteLine("Test 'test_0_lmf_filter' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_lmf_filter().ToString() + "'");
            }
            if (Tests.test_8_local_deadce_causes() != 8)
            {
                Console.WriteLine("Test 'test_8_local_deadce_causes' didn't return expected value. Expected: '8' Got: '" + Tests.test_8_local_deadce_causes().ToString() + "'");
            }
            if (Tests.test_0_except_opt_two_clauses() != 0)
            {
                Console.WriteLine("Test 'test_0_except_opt_two_clauses' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_except_opt_two_clauses().ToString() + "'");
            }
            if (Tests.test_100_long_vars_in_clauses_initlocals_opt() != 100)
            {
                Console.WriteLine("Test 'test_100_long_vars_in_clauses_initlocals_opt' didn't return expected value. Expected: '100' Got: '" + Tests.test_100_long_vars_in_clauses_initlocals_opt().ToString() + "'");
            }
            if (Tests.test_0_ldflda_null() != 0)
            {
                Console.WriteLine("Test 'test_0_ldflda_null' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_ldflda_null().ToString() + "'");
            }
            if (Tests.test_0_ldflda_null_pointer() != 0)
            {
                Console.WriteLine("Test 'test_0_ldflda_null_pointer' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_ldflda_null_pointer().ToString() + "'");
            }
            if (Tests.test_0_many_nested_loops() != 0)
            {
                Console.WriteLine("Test 'test_0_many_nested_loops' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_many_nested_loops().ToString() + "'");
            }
            if (Tests.test_0_logic_run() != 0)
            {
                Console.WriteLine("Test 'test_0_logic_run' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_logic_run().ToString() + "'");
            }
            if (Tests.test_1028_sieve() != 1028)
            {
                Console.WriteLine("Test 'test_1028_sieve' didn't return expected value. Expected: '1028' Got: '" + Tests.test_1028_sieve().ToString() + "'");
            }
            if (Tests.test_3524578_fib() != 3524578)
            {
                Console.WriteLine("Test 'test_3524578_fib' didn't return expected value. Expected: '3524578' Got: '" + Tests.test_3524578_fib().ToString() + "'");
            }
            if (Tests.test_0_hanoi() != 0)
            {
                Console.WriteLine("Test 'test_0_hanoi' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_hanoi().ToString() + "'");
            }
            if (Tests.test_0_castclass() != 0)
            {
                Console.WriteLine("Test 'test_0_castclass' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_castclass().ToString() + "'");
            }
            if (Tests.test_23005000_float() != 23005000)
            {
                Console.WriteLine("Test 'test_23005000_float' didn't return expected value. Expected: '23005000' Got: '" + Tests.test_23005000_float().ToString() + "'");
            }
            if (Tests.test_0_return_Basic() != 0)
            {
                Console.WriteLine("Test 'test_0_return_Basic' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_return_Basic().ToString() + "'");
            }
            if (Tests.test_100000_return_large() != 100000)
            {
                Console.WriteLine("Test 'test_100000_return_large' didn't return expected value. Expected: '100000' Got: '" + Tests.test_100000_return_large().ToString() + "'");
            }
            if (Tests.test_1_load_bool() != 1)
            {
                Console.WriteLine("Test 'test_1_load_bool' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_load_bool().ToString() + "'");
            }
            if (Tests.test_0_load_bool_false() != 0)
            {
                Console.WriteLine("Test 'test_0_load_bool_false' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_load_bool_false().ToString() + "'");
            }
            if (Tests.test_200_load_byte() != 200)
            {
                Console.WriteLine("Test 'test_200_load_byte' didn't return expected value. Expected: '200' Got: '" + Tests.test_200_load_byte().ToString() + "'");
            }
            if (Tests.test_100_load_sbyte() != 100)
            {
                Console.WriteLine("Test 'test_100_load_sbyte' didn't return expected value. Expected: '100' Got: '" + Tests.test_100_load_sbyte().ToString() + "'");
            }
            if (Tests.test_200_load_short() != 200)
            {
                Console.WriteLine("Test 'test_200_load_short' didn't return expected value. Expected: '200' Got: '" + Tests.test_200_load_short().ToString() + "'");
            }
            if (Tests.test_100_load_ushort() != 100)
            {
                Console.WriteLine("Test 'test_100_load_ushort' didn't return expected value. Expected: '100' Got: '" + Tests.test_100_load_ushort().ToString() + "'");
            }
            if (Tests.test_3_add_simple() != 3)
            {
                Console.WriteLine("Test 'test_3_add_simple' didn't return expected value. Expected: '3' Got: '" + Tests.test_3_add_simple().ToString() + "'");
            }
            if (Tests.test_3_add_imm() != 3)
            {
                Console.WriteLine("Test 'test_3_add_imm' didn't return expected value. Expected: '3' Got: '" + Tests.test_3_add_imm().ToString() + "'");
            }
            if (Tests.test_13407573_add_largeimm() != 13407573)
            {
                Console.WriteLine("Test 'test_13407573_add_largeimm' didn't return expected value. Expected: '13407573' Got: '" + Tests.test_13407573_add_largeimm().ToString() + "'");
            }
            if (Tests.test_1_sub_simple() != 1)
            {
                Console.WriteLine("Test 'test_1_sub_simple' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_sub_simple().ToString() + "'");
            }
            if (Tests.test_1_sub_simple_un() != 1)
            {
                Console.WriteLine("Test 'test_1_sub_simple_un' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_sub_simple_un().ToString() + "'");
            }
            if (Tests.test_1_sub_imm() != 1)
            {
                Console.WriteLine("Test 'test_1_sub_imm' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_sub_imm().ToString() + "'");
            }
            if (Tests.test_2_sub_large_imm() != 2)
            {
                Console.WriteLine("Test 'test_2_sub_large_imm' didn't return expected value. Expected: '2' Got: '" + Tests.test_2_sub_large_imm().ToString() + "'");
            }
            if (Tests.test_0_sub_inv_imm() != 0)
            {
                Console.WriteLine("Test 'test_0_sub_inv_imm' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_sub_inv_imm().ToString() + "'");
            }
            if (Tests.test_2_and() != 2)
            {
                Console.WriteLine("Test 'test_2_and' didn't return expected value. Expected: '2' Got: '" + Tests.test_2_and().ToString() + "'");
            }
            if (Tests.test_0_and_imm() != 0)
            {
                Console.WriteLine("Test 'test_0_and_imm' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_and_imm().ToString() + "'");
            }
            if (Tests.test_0_and_large_imm() != 0)
            {
                Console.WriteLine("Test 'test_0_and_large_imm' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_and_large_imm().ToString() + "'");
            }
            if (Tests.test_0_and_large_imm2() != 0)
            {
                Console.WriteLine("Test 'test_0_and_large_imm2' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_and_large_imm2().ToString() + "'");
            }
            if (Tests.test_2_div() != 2)
            {
                Console.WriteLine("Test 'test_2_div' didn't return expected value. Expected: '2' Got: '" + Tests.test_2_div().ToString() + "'");
            }
            if (Tests.test_4_div_imm() != 4)
            {
                Console.WriteLine("Test 'test_4_div_imm' didn't return expected value. Expected: '4' Got: '" + Tests.test_4_div_imm().ToString() + "'");
            }
            if (Tests.test_4_divun_imm() != 4)
            {
                Console.WriteLine("Test 'test_4_divun_imm' didn't return expected value. Expected: '4' Got: '" + Tests.test_4_divun_imm().ToString() + "'");
            }
            if (Tests.test_0_div_fold() != 0)
            {
                Console.WriteLine("Test 'test_0_div_fold' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_div_fold().ToString() + "'");
            }
            if (Tests.test_2_div_fold4() != 2)
            {
                Console.WriteLine("Test 'test_2_div_fold4' didn't return expected value. Expected: '2' Got: '" + Tests.test_2_div_fold4().ToString() + "'");
            }
            if (Tests.test_2_div_fold16() != 2)
            {
                Console.WriteLine("Test 'test_2_div_fold16' didn't return expected value. Expected: '2' Got: '" + Tests.test_2_div_fold16().ToString() + "'");
            }
            if (Tests.test_719177_div_destreg() != 719177)
            {
                Console.WriteLine("Test 'test_719177_div_destreg' didn't return expected value. Expected: '719177' Got: '" + Tests.test_719177_div_destreg().ToString() + "'");
            }
            if (Tests.test_1_remun_imm() != 1)
            {
                Console.WriteLine("Test 'test_1_remun_imm' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_remun_imm().ToString() + "'");
            }
            if (Tests.test_2_bigremun_imm() != 2)
            {
                Console.WriteLine("Test 'test_2_bigremun_imm' didn't return expected value. Expected: '2' Got: '" + Tests.test_2_bigremun_imm().ToString() + "'");
            }
            if (Tests.test_2_rem_Basic() != 2)
            {
                Console.WriteLine("Test 'test_2_rem_Basic' didn't return expected value. Expected: '2' Got: '" + Tests.test_2_rem_Basic().ToString() + "'");
            }
            if (Tests.test_4_rem_imm() != 4)
            {
                Console.WriteLine("Test 'test_4_rem_imm' didn't return expected value. Expected: '4' Got: '" + Tests.test_4_rem_imm().ToString() + "'");
            }
            if (Tests.test_0_rem_imm_0() != 0)
            {
                Console.WriteLine("Test 'test_0_rem_imm_0' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_rem_imm_0().ToString() + "'");
            }
            if (Tests.test_0_rem_imm_0_neg() != 0)
            {
                Console.WriteLine("Test 'test_0_rem_imm_0_neg' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_rem_imm_0_neg().ToString() + "'");
            }
            if (Tests.test_4_rem_big_imm() != 4)
            {
                Console.WriteLine("Test 'test_4_rem_big_imm' didn't return expected value. Expected: '4' Got: '" + Tests.test_4_rem_big_imm().ToString() + "'");
            }
            if (Tests.test_9_mul() != 9)
            {
                Console.WriteLine("Test 'test_9_mul' didn't return expected value. Expected: '9' Got: '" + Tests.test_9_mul().ToString() + "'");
            }
            if (Tests.test_15_mul_imm() != 15)
            {
                Console.WriteLine("Test 'test_15_mul_imm' didn't return expected value. Expected: '15' Got: '" + Tests.test_15_mul_imm().ToString() + "'");
            }
            if (Tests.test_24_mul_Basic() != 24)
            {
                Console.WriteLine("Test 'test_24_mul_Basic' didn't return expected value. Expected: '24' Got: '" + Tests.test_24_mul_Basic().ToString() + "'");
            }
            if (Tests.test_24_mul_ovf_Basic() != 24)
            {
                Console.WriteLine("Test 'test_24_mul_ovf_Basic' didn't return expected value. Expected: '24' Got: '" + Tests.test_24_mul_ovf_Basic().ToString() + "'");
            }
            if (Tests.test_24_mul_un_Basic() != 24)
            {
                Console.WriteLine("Test 'test_24_mul_un_Basic' didn't return expected value. Expected: '24' Got: '" + Tests.test_24_mul_un_Basic().ToString() + "'");
            }
            if (Tests.test_24_mul_ovf_un_Basic() != 24)
            {
                Console.WriteLine("Test 'test_24_mul_ovf_un_Basic' didn't return expected value. Expected: '24' Got: '" + Tests.test_24_mul_ovf_un_Basic().ToString() + "'");
            }
            if (Tests.test_0_add_ovf1() != 0)
            {
                Console.WriteLine("Test 'test_0_add_ovf1' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_add_ovf1().ToString() + "'");
            }
            if (Tests.test_528_mark_runlength_large() != 528)
            {
                Console.WriteLine("Test 'test_528_mark_runlength_large' didn't return expected value. Expected: '528' Got: '" + Tests.test_528_mark_runlength_large().ToString() + "'");
            }
            if (Tests.test_0_liveness_2() != 0)
            {
                Console.WriteLine("Test 'test_0_liveness_2' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_liveness_2().ToString() + "'");
            }
            if (Tests.test_0_liveness_3() != 0)
            {
                Console.WriteLine("Test 'test_0_liveness_3' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_liveness_3().ToString() + "'");
            }
            if (Tests.test_0_liveness_4() != 0)
            {
                Console.WriteLine("Test 'test_0_liveness_4' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_liveness_4().ToString() + "'");
            }
            if (Tests.test_0_liveness_5() != 0)
            {
                Console.WriteLine("Test 'test_0_liveness_5' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_liveness_5().ToString() + "'");
            }
            if (Tests.test_0_liveness_6() != 0)
            {
                Console.WriteLine("Test 'test_0_liveness_6' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_liveness_6().ToString() + "'");
            }
            if (Tests.test_0_multi_dim_ref_array_wbarrier() != 0)
            {
                Console.WriteLine("Test 'test_0_multi_dim_ref_array_wbarrier' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_multi_dim_ref_array_wbarrier().ToString() + "'");
            }
            if (Tests.test_0_liveness_7() != 0)
            {
                Console.WriteLine("Test 'test_0_liveness_7' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_liveness_7().ToString() + "'");
            }
            if (Tests.test_0_liveness_8() != 0)
            {
                Console.WriteLine("Test 'test_0_liveness_8' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_liveness_8().ToString() + "'");
            }
            if (Tests.test_0_liveness_9() != 0)
            {
                Console.WriteLine("Test 'test_0_liveness_9' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_liveness_9().ToString() + "'");
            }
            if (Tests.test_0_liveness_10() != 0)
            {
                Console.WriteLine("Test 'test_0_liveness_10' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_liveness_10().ToString() + "'");
            }
            if (Tests.test_0_liveness_11() != 0)
            {
                Console.WriteLine("Test 'test_0_liveness_11' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_liveness_11().ToString() + "'");
            }
            if (Tests.test_0_liveness_12() != 0)
            {
                Console.WriteLine("Test 'test_0_liveness_12' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_liveness_12().ToString() + "'");
            }
            if (Tests.test_0_liveness_13() != 0)
            {
                Console.WriteLine("Test 'test_0_liveness_13' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_liveness_13().ToString() + "'");
            }
            if (Tests.test_0_catch() != 0)
            {
                Console.WriteLine("Test 'test_0_catch' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_catch().ToString() + "'");
            }
            if (Tests.test_0_finally_without_exc() != 0)
            {
                Console.WriteLine("Test 'test_0_finally_without_exc' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_finally_without_exc().ToString() + "'");
            }
            if (Tests.test_0_finally() != 0)
            {
                Console.WriteLine("Test 'test_0_finally' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_finally().ToString() + "'");
            }
            if (Tests.test_0_nested_finally() != 0)
            {
                Console.WriteLine("Test 'test_0_nested_finally' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_nested_finally().ToString() + "'");
            }
            if (Tests.test_0_byte_cast() != 0)
            {
                Console.WriteLine("Test 'test_0_byte_cast' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_byte_cast().ToString() + "'");
            }
            if (Tests.test_0_sbyte_cast() != 0)
            {
                Console.WriteLine("Test 'test_0_sbyte_cast' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_sbyte_cast().ToString() + "'");
            }
            if (Tests.test_0_ushort_cast() != 0)
            {
                Console.WriteLine("Test 'test_0_ushort_cast' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_ushort_cast().ToString() + "'");
            }
            if (Tests.test_0_short_cast() != 0)
            {
                Console.WriteLine("Test 'test_0_short_cast' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_short_cast().ToString() + "'");
            }
            if (Tests.test_0_int_cast() != 0)
            {
                Console.WriteLine("Test 'test_0_int_cast' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_int_cast().ToString() + "'");
            }
            if (Tests.test_0_uint_cast() != 0)
            {
                Console.WriteLine("Test 'test_0_uint_cast' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_uint_cast().ToString() + "'");
            }
            if (Tests.test_0_long_cast() != 0)
            {
                Console.WriteLine("Test 'test_0_long_cast' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_long_cast().ToString() + "'");
            }
            if (Tests.test_0_ulong_cast() != 0)
            {
                Console.WriteLine("Test 'test_0_ulong_cast' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_ulong_cast().ToString() + "'");
            }
            if (Tests.test_0_simple_double_casts() != 0)
            {
                Console.WriteLine("Test 'test_0_simple_double_casts' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_simple_double_casts().ToString() + "'");
            }
            if (Tests.test_0_div_zero() != 0)
            {
                Console.WriteLine("Test 'test_0_div_zero' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_div_zero().ToString() + "'");
            }
            if (Tests.test_0_cfold_div_zero() != 0)
            {
                Console.WriteLine("Test 'test_0_cfold_div_zero' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_cfold_div_zero().ToString() + "'");
            }
            if (Tests.test_0_udiv_zero() != 0)
            {
                Console.WriteLine("Test 'test_0_udiv_zero' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_udiv_zero().ToString() + "'");
            }
            if (Tests.test_0_long_div_zero() != 0)
            {
                Console.WriteLine("Test 'test_0_long_div_zero' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_long_div_zero().ToString() + "'");
            }
            if (Tests.test_0_ulong_div_zero() != 0)
            {
                Console.WriteLine("Test 'test_0_ulong_div_zero' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_ulong_div_zero().ToString() + "'");
            }
            if (Tests.test_0_float_div_zero() != 0)
            {
                Console.WriteLine("Test 'test_0_float_div_zero' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_float_div_zero().ToString() + "'");
            }
            if (Tests.test_0_invalid_unbox() != 0)
            {
                Console.WriteLine("Test 'test_0_invalid_unbox' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_invalid_unbox().ToString() + "'");
            }
            if (Tests.test_0_invalid_unbox_arrays() != 0)
            {
                Console.WriteLine("Test 'test_0_invalid_unbox_arrays' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_invalid_unbox_arrays().ToString() + "'");
            }
            if (Tests.test_2_multiple_finally_clauses() != 2)
            {
                Console.WriteLine("Test 'test_2_multiple_finally_clauses' didn't return expected value. Expected: '2' Got: '" + Tests.test_2_multiple_finally_clauses().ToString() + "'");
            }
            if (Tests.test_3_checked_cast_un_Exceptions() != 3)
            {
                Console.WriteLine("Test 'test_3_checked_cast_un_Exceptions' didn't return expected value. Expected: '3' Got: '" + Tests.test_3_checked_cast_un_Exceptions().ToString() + "'");
            }
            if (Tests.test_4_checked_cast_Exceptions() != 4)
            {
                Console.WriteLine("Test 'test_4_checked_cast_Exceptions' didn't return expected value. Expected: '4' Got: '" + Tests.test_4_checked_cast_Exceptions().ToString() + "'");
            }
            if (Tests.test_0_multi_dim_array_access() != 0)
            {
                Console.WriteLine("Test 'test_0_multi_dim_array_access' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_multi_dim_array_access().ToString() + "'");
            }
            if (Tests.test_2_array_mismatch() != 2)
            {
                Console.WriteLine("Test 'test_2_array_mismatch' didn't return expected value. Expected: '2' Got: '" + Tests.test_2_array_mismatch().ToString() + "'");
            }
            if (Tests.test_0_ovf1() != 0)
            {
                Console.WriteLine("Test 'test_0_ovf1' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_ovf1().ToString() + "'");
            }
            if (Tests.test_1_ovf2() != 1)
            {
                Console.WriteLine("Test 'test_1_ovf2' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_ovf2().ToString() + "'");
            }
            if (Tests.test_0_ovf3() != 0)
            {
                Console.WriteLine("Test 'test_0_ovf3' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_ovf3().ToString() + "'");
            }
            if (Tests.test_1_ovf4() != 1)
            {
                Console.WriteLine("Test 'test_1_ovf4' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_ovf4().ToString() + "'");
            }
            if (Tests.test_0_ovf5() != 0)
            {
                Console.WriteLine("Test 'test_0_ovf5' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_ovf5().ToString() + "'");
            }
            if (Tests.test_1_ovf6() != 1)
            {
                Console.WriteLine("Test 'test_1_ovf6' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_ovf6().ToString() + "'");
            }
            if (Tests.test_0_ovf7() != 0)
            {
                Console.WriteLine("Test 'test_0_ovf7' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_ovf7().ToString() + "'");
            }
            if (Tests.test_1_ovf8() != 1)
            {
                Console.WriteLine("Test 'test_1_ovf8' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_ovf8().ToString() + "'");
            }
            if (Tests.test_0_ovf9() != 0)
            {
                Console.WriteLine("Test 'test_0_ovf9' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_ovf9().ToString() + "'");
            }
            if (Tests.test_1_ovf10() != 1)
            {
                Console.WriteLine("Test 'test_1_ovf10' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_ovf10().ToString() + "'");
            }
            if (Tests.test_0_ovf11() != 0)
            {
                Console.WriteLine("Test 'test_0_ovf11' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_ovf11().ToString() + "'");
            }
            if (Tests.test_1_ovf12() != 1)
            {
                Console.WriteLine("Test 'test_1_ovf12' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_ovf12().ToString() + "'");
            }
            if (Tests.test_0_ovf13() != 0)
            {
                Console.WriteLine("Test 'test_0_ovf13' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_ovf13().ToString() + "'");
            }
            if (Tests.test_1_ovf14() != 1)
            {
                Console.WriteLine("Test 'test_1_ovf14' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_ovf14().ToString() + "'");
            }
            if (Tests.test_0_ovf15() != 0)
            {
                Console.WriteLine("Test 'test_0_ovf15' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_ovf15().ToString() + "'");
            }
            if (Tests.test_1_ovf16() != 1)
            {
                Console.WriteLine("Test 'test_1_ovf16' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_ovf16().ToString() + "'");
            }
            if (Tests.test_0_ovf17() != 0)
            {
                Console.WriteLine("Test 'test_0_ovf17' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_ovf17().ToString() + "'");
            }
            if (Tests.test_0_ovf18() != 0)
            {
                Console.WriteLine("Test 'test_0_ovf18' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_ovf18().ToString() + "'");
            }
            if (Tests.test_1_ovf19() != 1)
            {
                Console.WriteLine("Test 'test_1_ovf19' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_ovf19().ToString() + "'");
            }
            if (Tests.test_0_ovf20() != 0)
            {
                Console.WriteLine("Test 'test_0_ovf20' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_ovf20().ToString() + "'");
            }
            if (Tests.test_1_ovf21() != 1)
            {
                Console.WriteLine("Test 'test_1_ovf21' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_ovf21().ToString() + "'");
            }
            if (Tests.test_1_ovf22() != 1)
            {
                Console.WriteLine("Test 'test_1_ovf22' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_ovf22().ToString() + "'");
            }
            if (Tests.test_1_ovf23() != 1)
            {
                Console.WriteLine("Test 'test_1_ovf23' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_ovf23().ToString() + "'");
            }
            if (Tests.test_0_exception_in_cctor() != 0)
            {
                Console.WriteLine("Test 'test_0_exception_in_cctor' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_exception_in_cctor().ToString() + "'");
            }
            if (Tests.test_5_regalloc() != 5)
            {
                Console.WriteLine("Test 'test_5_regalloc' didn't return expected value. Expected: '5' Got: '" + Tests.test_5_regalloc().ToString() + "'");
            }
            if (Tests.test_0_rethrow_nested() != 0)
            {
                Console.WriteLine("Test 'test_0_rethrow_nested' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_rethrow_nested().ToString() + "'");
            }
            if (Tests.test_0_rethrow_stacktrace() != 0)
            {
                Console.WriteLine("Test 'test_0_rethrow_stacktrace' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_rethrow_stacktrace().ToString() + "'");
            }
            if (Tests.test_0_struct3_args() != 0)
            {
                Console.WriteLine("Test 'test_0_struct3_args' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_struct3_args().ToString() + "'");
            }
            if (Tests.test_0_struct4_args() != 0)
            {
                Console.WriteLine("Test 'test_0_struct4_args' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_struct4_args().ToString() + "'");
            }
            if (Tests.test_44_unbox_trampoline() != 44)
            {
                Console.WriteLine("Test 'test_44_unbox_trampoline' didn't return expected value. Expected: '44' Got: '" + Tests.test_44_unbox_trampoline().ToString() + "'");
            }
            if (Tests.test_0_unbox_trampoline2() != 0)
            {
                Console.WriteLine("Test 'test_0_unbox_trampoline2' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_unbox_trampoline2().ToString() + "'");
            }
            if (Tests.test_0_fields_with_big_offsets() != 0)
            {
                Console.WriteLine("Test 'test_0_fields_with_big_offsets' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_fields_with_big_offsets().ToString() + "'");
            }
            if (Tests.test_0_seektest() != 0)
            {
                Console.WriteLine("Test 'test_0_seektest' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_seektest().ToString() + "'");
            }
            if (Tests.test_0_null_cast() != 0)
            {
                Console.WriteLine("Test 'test_0_null_cast' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_null_cast().ToString() + "'");
            }
            if (Tests.test_0_super_cast() != 0)
            {
                Console.WriteLine("Test 'test_0_super_cast' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_super_cast().ToString() + "'");
            }
            if (Tests.test_0_super_cast_array() != 0)
            {
                Console.WriteLine("Test 'test_0_super_cast_array' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_super_cast_array().ToString() + "'");
            }
            if (Tests.test_0_multi_array_cast() != 0)
            {
                Console.WriteLine("Test 'test_0_multi_array_cast' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_multi_array_cast().ToString() + "'");
            }
            if (Tests.test_0_vector_array_cast() != 0)
            {
                Console.WriteLine("Test 'test_0_vector_array_cast' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_vector_array_cast().ToString() + "'");
            }
            if (Tests.test_0_enum_array_cast() != 0)
            {
                Console.WriteLine("Test 'test_0_enum_array_cast' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_enum_array_cast().ToString() + "'");
            }
            if (Tests.test_0_more_cast_corner_cases() != 0)
            {
                Console.WriteLine("Test 'test_0_more_cast_corner_cases' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_more_cast_corner_cases().ToString() + "'");
            }
            if (Tests.test_0_cast_iface_array() != 0)
            {
                Console.WriteLine("Test 'test_0_cast_iface_array' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_cast_iface_array().ToString() + "'");
            }
            if (Tests.test_719162_complex_div() != 719162)
            {
                Console.WriteLine("Test 'test_719162_complex_div' didn't return expected value. Expected: '719162' Got: '" + Tests.test_719162_complex_div().ToString() + "'");
            }
            if (Tests.test_2_static_delegate() != 2)
            {
                Console.WriteLine("Test 'test_2_static_delegate' didn't return expected value. Expected: '2' Got: '" + Tests.test_2_static_delegate().ToString() + "'");
            }
            if (Tests.test_2_instance_delegate() != 2)
            {
                Console.WriteLine("Test 'test_2_instance_delegate' didn't return expected value. Expected: '2' Got: '" + Tests.test_2_instance_delegate().ToString() + "'");
            }
            if (Tests.test_1_store_decimal() != 1)
            {
                Console.WriteLine("Test 'test_1_store_decimal' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_store_decimal().ToString() + "'");
            }
            if (Tests.test_2_intptr_stobj() != 2)
            {
                Console.WriteLine("Test 'test_2_intptr_stobj' didn't return expected value. Expected: '2' Got: '" + Tests.test_2_intptr_stobj().ToString() + "'");
            }
            if (Tests.test_155_regalloc() != 155)
            {
                Console.WriteLine("Test 'test_155_regalloc' didn't return expected value. Expected: '155' Got: '" + Tests.test_155_regalloc().ToString() + "'");
            }
            if (Tests.test_2_large_struct_pass() != 2)
            {
                Console.WriteLine("Test 'test_2_large_struct_pass' didn't return expected value. Expected: '2' Got: '" + Tests.test_2_large_struct_pass().ToString() + "'");
            }
            if (Tests.test_0_pin_string() != 0)
            {
                Console.WriteLine("Test 'test_0_pin_string' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_pin_string().ToString() + "'");
            }
            if (Tests.test_0_and_cmp_static() != 0)
            {
                Console.WriteLine("Test 'test_0_and_cmp_static' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_and_cmp_static().ToString() + "'");
            }
            if (Tests.test_0_byte_compares() != 0)
            {
                Console.WriteLine("Test 'test_0_byte_compares' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_byte_compares().ToString() + "'");
            }
            if (Tests.test_71_long_shift_right() != 71)
            {
                Console.WriteLine("Test 'test_71_long_shift_right' didn't return expected value. Expected: '71' Got: '" + Tests.test_71_long_shift_right().ToString() + "'");
            }
            if (Tests.test_0_addsub_mem() != 0)
            {
                Console.WriteLine("Test 'test_0_addsub_mem' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_addsub_mem().ToString() + "'");
            }
            if (Tests.test_0_sh32_mem() != 0)
            {
                Console.WriteLine("Test 'test_0_sh32_mem' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_sh32_mem().ToString() + "'");
            }
            if (Tests.test_0_long_arg_opt() != 0)
            {
                Console.WriteLine("Test 'test_0_long_arg_opt' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_long_arg_opt().ToString() + "'");
            }
            if (Tests.test_0_long_ret_opt() != 0)
            {
                Console.WriteLine("Test 'test_0_long_ret_opt' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_long_ret_opt().ToString() + "'");
            }
            if (Tests.test_0_cond_branch_side_effects() != 0)
            {
                Console.WriteLine("Test 'test_0_cond_branch_side_effects' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_cond_branch_side_effects().ToString() + "'");
            }
            if (Tests.test_0_arg_only_written() != 0)
            {
                Console.WriteLine("Test 'test_0_arg_only_written' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_arg_only_written().ToString() + "'");
            }
            if (Tests.test_4_static_inc_long() != 4)
            {
                Console.WriteLine("Test 'test_4_static_inc_long' didn't return expected value. Expected: '4' Got: '" + Tests.test_4_static_inc_long().ToString() + "'");
            }
            if (Tests.test_0_calls_opcode_emulation() != 0)
            {
                Console.WriteLine("Test 'test_0_calls_opcode_emulation' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_calls_opcode_emulation().ToString() + "'");
            }
            if (Tests.test_0_intrins_string_length() != 0)
            {
                Console.WriteLine("Test 'test_0_intrins_string_length' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_intrins_string_length().ToString() + "'");
            }
            if (Tests.test_0_intrins_string_chars() != 0)
            {
                Console.WriteLine("Test 'test_0_intrins_string_chars' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_intrins_string_chars().ToString() + "'");
            }
            if (Tests.test_0_intrins_object_gettype() != 0)
            {
                Console.WriteLine("Test 'test_0_intrins_object_gettype' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_intrins_object_gettype().ToString() + "'");
            }
            if (Tests.test_0_intrins_object_gethashcode() != 0)
            {
                Console.WriteLine("Test 'test_0_intrins_object_gethashcode' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_intrins_object_gethashcode().ToString() + "'");
            }
            if (Tests.test_0_intrins_object_ctor() != 0)
            {
                Console.WriteLine("Test 'test_0_intrins_object_ctor' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_intrins_object_ctor().ToString() + "'");
            }
            if (Tests.test_0_intrins_array_rank() != 0)
            {
                Console.WriteLine("Test 'test_0_intrins_array_rank' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_intrins_array_rank().ToString() + "'");
            }
            if (Tests.test_0_intrins_array_length() != 0)
            {
                Console.WriteLine("Test 'test_0_intrins_array_length' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_intrins_array_length().ToString() + "'");
            }
            if (Tests.test_0_intrins_runtimehelpers_offset_to_string_data() != 0)
            {
                Console.WriteLine("Test 'test_0_intrins_runtimehelpers_offset_to_string_data' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_intrins_runtimehelpers_offset_to_string_data().ToString() + "'");
            }
            if (Tests.test_0_intrins_string_setchar() != 0)
            {
                Console.WriteLine("Test 'test_0_intrins_string_setchar' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_intrins_string_setchar().ToString() + "'");
            }
            if (Tests.test_0_regress_78990_unaligned_structs() != 0)
            {
                Console.WriteLine("Test 'test_0_regress_78990_unaligned_structs' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_regress_78990_unaligned_structs().ToString() + "'");
            }
            if (Tests.test_97_negative_index() != 97)
            {
                Console.WriteLine("Test 'test_97_negative_index' didn't return expected value. Expected: '97' Got: '" + Tests.test_97_negative_index().ToString() + "'");
            }
            if (Tests.test_0_unsigned_right_shift_imm0() != 0)
            {
                Console.WriteLine("Test 'test_0_unsigned_right_shift_imm0' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_unsigned_right_shift_imm0().ToString() + "'");
            }
            if (Tests.test_0_abcrem_check_this_removal() != 0)
            {
                Console.WriteLine("Test 'test_0_abcrem_check_this_removal' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_abcrem_check_this_removal().ToString() + "'");
            }
            if (Tests.test_0_abcrem_check_this_removal2() != 0)
            {
                Console.WriteLine("Test 'test_0_abcrem_check_this_removal2' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_abcrem_check_this_removal2().ToString() + "'");
            }
            if (Tests.test_0_array_access_64_bit() != 0)
            {
                Console.WriteLine("Test 'test_0_array_access_64_bit' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_array_access_64_bit().ToString() + "'");
            }
            if (Tests.test_0_float_return_spill() != 0)
            {
                Console.WriteLine("Test 'test_0_float_return_spill' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_float_return_spill().ToString() + "'");
            }
            if (Tests.test_0_ldsfld_soft_float() != 0)
            {
                Console.WriteLine("Test 'test_0_ldsfld_soft_float' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_ldsfld_soft_float().ToString() + "'");
            }
            if (Tests.test_0_ldfld_stfld_soft_float() != 0)
            {
                Console.WriteLine("Test 'test_0_ldfld_stfld_soft_float' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_ldfld_stfld_soft_float().ToString() + "'");
            }
            if (Tests.test_0_ldfld_stfld_soft_float_remote() != 0)
            {
                Console.WriteLine("Test 'test_0_ldfld_stfld_soft_float_remote' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_ldfld_stfld_soft_float_remote().ToString() + "'");
            }
            if (Tests.test_0_locals_soft_float() != 0)
            {
                Console.WriteLine("Test 'test_0_locals_soft_float' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_locals_soft_float().ToString() + "'");
            }
            if (Tests.test_0_vtype_arg_soft_float() != 0)
            {
                Console.WriteLine("Test 'test_0_vtype_arg_soft_float' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_vtype_arg_soft_float().ToString() + "'");
            }
            if (Tests.test_0_range_check_opt() != 0)
            {
                Console.WriteLine("Test 'test_0_range_check_opt' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_range_check_opt().ToString() + "'");
            }
            if (Tests.test_0_array_get_set_soft_float() != 0)
            {
                Console.WriteLine("Test 'test_0_array_get_set_soft_float' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_array_get_set_soft_float().ToString() + "'");
            }
            if (Tests.test_2_ldobj_stobj_optization() != 2)
            {
                Console.WriteLine("Test 'test_2_ldobj_stobj_optization' didn't return expected value. Expected: '2' Got: '" + Tests.test_2_ldobj_stobj_optization().ToString() + "'");
            }
            if (Tests.test_0_vtype_phi() != 0)
            {
                Console.WriteLine("Test 'test_0_vtype_phi' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_vtype_phi().ToString() + "'");
            }
            if (Tests.test_0_llvm_moving_faulting_loads() != 0)
            {
                Console.WriteLine("Test 'test_0_llvm_moving_faulting_loads' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_llvm_moving_faulting_loads().ToString() + "'");
            }
            if (Tests.test_0_multiple_cctor_calls_regress_679467() != 0)
            {
                Console.WriteLine("Test 'test_0_multiple_cctor_calls_regress_679467' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_multiple_cctor_calls_regress_679467().ToString() + "'");
            }
            if (Tests.test_36_simple() != 36)
            {
                Console.WriteLine("Test 'test_36_simple' didn't return expected value. Expected: '36' Got: '" + Tests.test_36_simple().ToString() + "'");
            }
            if (Tests.test_36_liveness() != 36)
            {
                Console.WriteLine("Test 'test_36_liveness' didn't return expected value. Expected: '36' Got: '" + Tests.test_36_liveness().ToString() + "'");
            }
            if (Tests.test_4_vtype() != 4)
            {
                Console.WriteLine("Test 'test_4_vtype' didn't return expected value. Expected: '4' Got: '" + Tests.test_4_vtype().ToString() + "'");
            }
            if (Tests.test_0_return() != 0)
            {
                Console.WriteLine("Test 'test_0_return' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_return().ToString() + "'");
            }
            if (Tests.test_2_int_return() != 2)
            {
                Console.WriteLine("Test 'test_2_int_return' didn't return expected value. Expected: '2' Got: '" + Tests.test_2_int_return().ToString() + "'");
            }
            if (Tests.test_1_int_pass() != 1)
            {
                Console.WriteLine("Test 'test_1_int_pass' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_int_pass().ToString() + "'");
            }
            if (Tests.test_1_int_pass_many() != 1)
            {
                Console.WriteLine("Test 'test_1_int_pass_many' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_int_pass_many().ToString() + "'");
            }
            if (Tests.test_2_inline_saved_arg_type() != 2)
            {
                Console.WriteLine("Test 'test_2_inline_saved_arg_type' didn't return expected value. Expected: '2' Got: '" + Tests.test_2_inline_saved_arg_type().ToString() + "'");
            }
            if (Tests.test_5_pass_longs() != 5)
            {
                Console.WriteLine("Test 'test_5_pass_longs' didn't return expected value. Expected: '5' Got: '" + Tests.test_5_pass_longs().ToString() + "'");
            }
            if (Tests.test_55_pass_even_more() != 55)
            {
                Console.WriteLine("Test 'test_55_pass_even_more' didn't return expected value. Expected: '55' Got: '" + Tests.test_55_pass_even_more().ToString() + "'");
            }
            if (Tests.test_1_sparc_argument_passing() != 1)
            {
                Console.WriteLine("Test 'test_1_sparc_argument_passing' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_sparc_argument_passing().ToString() + "'");
            }
            if (Tests.test_21_sparc_byte_argument_passing() != 21)
            {
                Console.WriteLine("Test 'test_21_sparc_byte_argument_passing' didn't return expected value. Expected: '21' Got: '" + Tests.test_21_sparc_byte_argument_passing().ToString() + "'");
            }
            if (Tests.test_21_sparc_sbyte_argument_passing() != 21)
            {
                Console.WriteLine("Test 'test_21_sparc_sbyte_argument_passing' didn't return expected value. Expected: '21' Got: '" + Tests.test_21_sparc_sbyte_argument_passing().ToString() + "'");
            }
            if (Tests.test_21_sparc_short_argument_passing() != 21)
            {
                Console.WriteLine("Test 'test_21_sparc_short_argument_passing' didn't return expected value. Expected: '21' Got: '" + Tests.test_21_sparc_short_argument_passing().ToString() + "'");
            }
            if (Tests.test_721_sparc_float_argument_passing() != 721)
            {
                Console.WriteLine("Test 'test_721_sparc_float_argument_passing' didn't return expected value. Expected: '721' Got: '" + Tests.test_721_sparc_float_argument_passing().ToString() + "'");
            }
            if (Tests.test_55_sparc_float_argument_passing2() != 55)
            {
                Console.WriteLine("Test 'test_55_sparc_float_argument_passing2' didn't return expected value. Expected: '55' Got: '" + Tests.test_55_sparc_float_argument_passing2().ToString() + "'");
            }
            if (Tests.test_0_float_argument_passing_precision() != 0)
            {
                Console.WriteLine("Test 'test_0_float_argument_passing_precision' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_float_argument_passing_precision().ToString() + "'");
            }
            if (Tests.test_2_sparc_takeaddr_argument_passing() != 2)
            {
                Console.WriteLine("Test 'test_2_sparc_takeaddr_argument_passing' didn't return expected value. Expected: '2' Got: '" + Tests.test_2_sparc_takeaddr_argument_passing().ToString() + "'");
            }
            if (Tests.test_721_sparc_takeaddr_argument_passing2() != 721)
            {
                Console.WriteLine("Test 'test_721_sparc_takeaddr_argument_passing2' didn't return expected value. Expected: '721' Got: '" + Tests.test_721_sparc_takeaddr_argument_passing2().ToString() + "'");
            }
            if (Tests.test_0_sparc_byref_double_argument_passing() != 0)
            {
                Console.WriteLine("Test 'test_0_sparc_byref_double_argument_passing' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_sparc_byref_double_argument_passing().ToString() + "'");
            }
            if (Tests.test_0_long_arg_assign() != 0)
            {
                Console.WriteLine("Test 'test_0_long_arg_assign' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_long_arg_assign().ToString() + "'");
            }
            if (Tests.test_0_ptr_return() != 0)
            {
                Console.WriteLine("Test 'test_0_ptr_return' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_ptr_return().ToString() + "'");
            }
            if (Tests.test_0_isnan() != 0)
            {
                Console.WriteLine("Test 'test_0_isnan' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_isnan().ToString() + "'");
            }
            if (Tests.test_1_handle_dup_stloc() != 1)
            {
                Console.WriteLine("Test 'test_1_handle_dup_stloc' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_handle_dup_stloc().ToString() + "'");
            }
            if (Tests.test_3_long_ret() != 3)
            {
                Console.WriteLine("Test 'test_3_long_ret' didn't return expected value. Expected: '3' Got: '" + Tests.test_3_long_ret().ToString() + "'");
            }
            if (Tests.test_1_long_ret2() != 1)
            {
                Console.WriteLine("Test 'test_1_long_ret2' didn't return expected value. Expected: '1' Got: '" + Tests.test_1_long_ret2().ToString() + "'");
            }
            if (Tests.test_0_sparc_long_ret_regress_541577() != 0)
            {
                Console.WriteLine("Test 'test_0_sparc_long_ret_regress_541577' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_sparc_long_ret_regress_541577().ToString() + "'");
            }
            if (Tests.test_0_ftol_clobber() != 0)
            {
                Console.WriteLine("Test 'test_0_ftol_clobber' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_ftol_clobber().ToString() + "'");
            }
            if (Tests.test_0_return_Objects() != 0)
            {
                Console.WriteLine("Test 'test_0_return_Objects' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_return_Objects().ToString() + "'");
            }
            if (Tests.test_0_string_access() != 0)
            {
                Console.WriteLine("Test 'test_0_string_access' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_string_access().ToString() + "'");
            }
            if (Tests.test_0_string_virtual_call() != 0)
            {
                Console.WriteLine("Test 'test_0_string_virtual_call' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_string_virtual_call().ToString() + "'");
            }
            if (Tests.test_0_iface_call() != 0)
            {
                Console.WriteLine("Test 'test_0_iface_call' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_iface_call().ToString() + "'");
            }
            if (Tests.test_5_newobj() != 5)
            {
                Console.WriteLine("Test 'test_5_newobj' didn't return expected value. Expected: '5' Got: '" + Tests.test_5_newobj().ToString() + "'");
            }
            if (Tests.test_4_box() != 4)
            {
                Console.WriteLine("Test 'test_4_box' didn't return expected value. Expected: '4' Got: '" + Tests.test_4_box().ToString() + "'");
            }
            if (Tests.test_0_enum_unbox() != 0)
            {
                Console.WriteLine("Test 'test_0_enum_unbox' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_enum_unbox().ToString() + "'");
            }
            if (Tests.test_3_return_struct() != 3)
            {
                Console.WriteLine("Test 'test_3_return_struct' didn't return expected value. Expected: '3' Got: '" + Tests.test_3_return_struct().ToString() + "'");
            }
            if (Tests.test_2_return_struct_virtual() != 2)
            {
                Console.WriteLine("Test 'test_2_return_struct_virtual' didn't return expected value. Expected: '2' Got: '" + Tests.test_2_return_struct_virtual().ToString() + "'");
            }
            if (Tests.test_5_pass_struct() != 5)
            {
                Console.WriteLine("Test 'test_5_pass_struct' didn't return expected value. Expected: '5' Got: '" + Tests.test_5_pass_struct().ToString() + "'");
            }
            if (Tests.test_5_pass_static_struct() != 5)
            {
                Console.WriteLine("Test 'test_5_pass_static_struct' didn't return expected value. Expected: '5' Got: '" + Tests.test_5_pass_static_struct().ToString() + "'");
            }
            if (Tests.test_5_pass_small_struct() != 5)
            {
                Console.WriteLine("Test 'test_5_pass_small_struct' didn't return expected value. Expected: '5' Got: '" + Tests.test_5_pass_small_struct().ToString() + "'");
            }
            if (Tests.test_0_struct1_args() != 0)
            {
                Console.WriteLine("Test 'test_0_struct1_args' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_struct1_args().ToString() + "'");
            }
            if (Tests.test_0_struct2_args() != 0)
            {
                Console.WriteLine("Test 'test_0_struct2_args' didn't return expected value. Expected: '0' Got: '" + Tests.test_0_struct2_args().ToString() + "'");
            }
            #endregion
        }

        public static void WriteError(string s)
        {
            Console.WriteLine("Error: " + s);
        }
    }
}

