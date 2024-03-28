using System;
using System.Runtime.InteropServices;
using System.Text;

namespace MegaLib.OS.Api
{
  using GLenum = UInt32;
  using GLboolean = Byte;
  using GLint = Int32;
  using GLsizei = Int32;
  using GLuint = UInt32;
  using GLfloat = Single;

  public class OpenGL32
  {
    #region Const

    public const uint GL_2D = 0x600;
    public const uint GL_2_BYTES = 0x1407;
    public const uint GL_3D = 0x601;
    public const uint GL_3D_COLOR = 0x602;
    public const uint GL_3D_COLOR_TEXTURE = 0x603;
    public const uint GL_3_BYTES = 0x1408;
    public const uint GL_4D_COLOR_TEXTURE = 0x604;
    public const uint GL_4_BYTES = 0x1409;
    public const uint GL_ACCUM = 0x100;
    public const uint GL_ACCUM_ALPHA_BITS = 0xd5b;
    public const uint GL_ACCUM_BLUE_BITS = 0xd5a;
    public const uint GL_ACCUM_BUFFER_BIT = 0x200;
    public const uint GL_ACCUM_CLEAR_VALUE = 0xb80;
    public const uint GL_ACCUM_GREEN_BITS = 0xd59;
    public const uint GL_ACCUM_RED_BITS = 0xd58;
    public const uint GL_ACTIVE_ATOMIC_COUNTER_BUFFERS = 0x92d9;
    public const uint GL_ACTIVE_ATTRIBUTES = 0x8b89;
    public const uint GL_ACTIVE_ATTRIBUTE_MAX_LENGTH = 0x8b8a;
    public const uint GL_ACTIVE_PROGRAM = 0x8259;
    public const uint GL_ACTIVE_RESOURCES = 0x92f5;
    public const uint GL_ACTIVE_SUBROUTINES = 0x8de5;
    public const uint GL_ACTIVE_SUBROUTINE_MAX_LENGTH = 0x8e48;
    public const uint GL_ACTIVE_SUBROUTINE_UNIFORMS = 0x8de6;
    public const uint GL_ACTIVE_SUBROUTINE_UNIFORM_LOCATIONS = 0x8e47;
    public const uint GL_ACTIVE_SUBROUTINE_UNIFORM_MAX_LENGTH = 0x8e49;
    public const uint GL_ACTIVE_TEXTURE = 0x84e0;
    public const uint GL_ACTIVE_UNIFORMS = 0x8b86;
    public const uint GL_ACTIVE_UNIFORM_BLOCKS = 0x8a36;
    public const uint GL_ACTIVE_UNIFORM_BLOCK_MAX_NAME_LENGTH = 0x8a35;
    public const uint GL_ACTIVE_UNIFORM_MAX_LENGTH = 0x8b87;
    public const uint GL_ACTIVE_VARIABLES = 0x9305;
    public const uint GL_ADD = 0x104;
    public const uint GL_ALIASED_LINE_WIDTH_RANGE = 0x846e;
    public const uint GL_ALL_ATTRIB_BITS = 0xfffff;
    public const uint GL_ALPHA = 0x1906;
    public const uint GL_ALPHA12 = 0x803d;
    public const uint GL_ALPHA16 = 0x803e;
    public const uint GL_ALPHA4 = 0x803b;
    public const uint GL_ALPHA8 = 0x803c;
    public const uint GL_ALPHA_BIAS = 0xd1d;
    public const uint GL_ALPHA_BITS = 0xd55;
    public const uint GL_ALPHA_SCALE = 0xd1c;
    public const uint GL_ALPHA_TEST = 0xbc0;
    public const uint GL_ALPHA_TEST_FUNC = 0xbc1;
    public const uint GL_ALPHA_TEST_REF = 0xbc2;
    public const uint GL_ALREADY_SIGNALED = 0x911a;
    public const uint GL_ALWAYS = 0x207;
    public const uint GL_AMBIENT = 0x1200;
    public const uint GL_AMBIENT_AND_DIFFUSE = 0x1602;
    public const uint GL_AND = 0x1501;
    public const uint GL_AND_INVERTED = 0x1504;
    public const uint GL_AND_REVERSE = 0x1502;
    public const uint GL_ANY_SAMPLES_PASSED = 0x8c2f;
    public const uint GL_ANY_SAMPLES_PASSED_CONSERVATIVE = 0x8d6a;
    public const uint GL_ARRAY_BUFFER = 0x8892;
    public const uint GL_ARRAY_BUFFER_BINDING = 0x8894;
    public const uint GL_ARRAY_SIZE = 0x92fb;
    public const uint GL_ARRAY_STRIDE = 0x92fe;
    public const uint GL_ATOMIC_COUNTER_BARRIER_BIT = 0x1000;
    public const uint GL_ATOMIC_COUNTER_BUFFER = 0x92c0;
    public const uint GL_ATOMIC_COUNTER_BUFFER_ACTIVE_ATOMIC_COUNTERS = 0x92c5;
    public const uint GL_ATOMIC_COUNTER_BUFFER_ACTIVE_ATOMIC_COUNTER_INDICES = 0x92c6;
    public const uint GL_ATOMIC_COUNTER_BUFFER_BINDING = 0x92c1;
    public const uint GL_ATOMIC_COUNTER_BUFFER_DATA_SIZE = 0x92c4;
    public const uint GL_ATOMIC_COUNTER_BUFFER_INDEX = 0x9301;
    public const uint GL_ATOMIC_COUNTER_BUFFER_REFERENCED_BY_COMPUTE_SHADER = 0x90ed;
    public const uint GL_ATOMIC_COUNTER_BUFFER_REFERENCED_BY_FRAGMENT_SHADER = 0x92cb;
    public const uint GL_ATOMIC_COUNTER_BUFFER_REFERENCED_BY_GEOMETRY_SHADER = 0x92ca;
    public const uint GL_ATOMIC_COUNTER_BUFFER_REFERENCED_BY_TESS_CONTROL_SHADER = 0x92c8;
    public const uint GL_ATOMIC_COUNTER_BUFFER_REFERENCED_BY_TESS_EVALUATION_SHADER = 0x92c9;
    public const uint GL_ATOMIC_COUNTER_BUFFER_REFERENCED_BY_VERTEX_SHADER = 0x92c7;
    public const uint GL_ATOMIC_COUNTER_BUFFER_SIZE = 0x92c3;
    public const uint GL_ATOMIC_COUNTER_BUFFER_START = 0x92c2;
    public const uint GL_ATTACHED_SHADERS = 0x8b85;
    public const uint GL_ATTRIB_STACK_DEPTH = 0xbb0;
    public const uint GL_AUTO_GENERATE_MIPMAP = 0x8295;
    public const uint GL_AUTO_NORMAL = 0xd80;
    public const uint GL_AUX0 = 0x409;
    public const uint GL_AUX1 = 0x40a;
    public const uint GL_AUX2 = 0x40b;
    public const uint GL_AUX3 = 0x40c;
    public const uint GL_AUX_BUFFERS = 0xc00;
    public const uint GL_BACK = 0x405;
    public const uint GL_BACK_LEFT = 0x402;
    public const uint GL_BACK_RIGHT = 0x403;
    public const uint GL_BGR = 0x80e0;
    public const uint GL_BGRA = 0x80e1;
    public const uint GL_BGRA_EXT = 0x80e1;
    public const uint GL_BGRA_INTEGER = 0x8d9b;
    public const uint GL_BGR_EXT = 0x80e0;
    public const uint GL_BGR_INTEGER = 0x8d9a;
    public const uint GL_BITMAP = 0x1a00;
    public const uint GL_BITMAP_TOKEN = 0x704;
    public const uint GL_BLEND = 0xbe2;
    public const uint GL_BLEND_COLOR = 0x8005;
    public const uint GL_BLEND_DST = 0xbe0;
    public const uint GL_BLEND_DST_ALPHA = 0x80ca;
    public const uint GL_BLEND_DST_RGB = 0x80c8;
    public const uint GL_BLEND_EQUATION = 0x8009;
    public const uint GL_BLEND_EQUATION_ALPHA = 0x883d;
    public const uint GL_BLEND_EQUATION_RGB = 0x8009;
    public const uint GL_BLEND_SRC = 0xbe1;
    public const uint GL_BLEND_SRC_ALPHA = 0x80cb;
    public const uint GL_BLEND_SRC_RGB = 0x80c9;
    public const uint GL_BLOCK_INDEX = 0x92fd;
    public const uint GL_BLUE = 0x1905;
    public const uint GL_BLUE_BIAS = 0xd1b;
    public const uint GL_BLUE_BITS = 0xd54;
    public const uint GL_BLUE_INTEGER = 0x8d96;
    public const uint GL_BLUE_SCALE = 0xd1a;
    public const uint GL_BOOL = 0x8b56;
    public const uint GL_BOOL_VEC2 = 0x8b57;
    public const uint GL_BOOL_VEC3 = 0x8b58;
    public const uint GL_BOOL_VEC4 = 0x8b59;
    public const uint GL_BUFFER = 0x82e0;
    public const uint GL_BUFFER_ACCESS = 0x88bb;
    public const uint GL_BUFFER_ACCESS_FLAGS = 0x911f;
    public const uint GL_BUFFER_BINDING = 0x9302;
    public const uint GL_BUFFER_DATA_SIZE = 0x9303;
    public const uint GL_BUFFER_IMMUTABLE_STORAGE = 0x821f;
    public const uint GL_BUFFER_MAPPED = 0x88bc;
    public const uint GL_BUFFER_MAP_LENGTH = 0x9120;
    public const uint GL_BUFFER_MAP_OFFSET = 0x9121;
    public const uint GL_BUFFER_MAP_POINTER = 0x88bd;
    public const uint GL_BUFFER_SIZE = 0x8764;
    public const uint GL_BUFFER_STORAGE_FLAGS = 0x8220;
    public const uint GL_BUFFER_UPDATE_BARRIER_BIT = 0x200;
    public const uint GL_BUFFER_USAGE = 0x8765;
    public const uint GL_BUFFER_VARIABLE = 0x92e5;
    public const uint GL_BYTE = 0x1400;
    public const uint GL_C3F_V3F = 0x2a24;
    public const uint GL_C4F_N3F_V3F = 0x2a26;
    public const uint GL_C4UB_V2F = 0x2a22;
    public const uint GL_C4UB_V3F = 0x2a23;
    public const uint GL_CAVEAT_SUPPORT = 0x82b8;
    public const uint GL_CCW = 0x901;
    public const uint GL_CLAMP = 0x2900;
    public const uint GL_CLAMP_READ_COLOR = 0x891c;
    public const uint GL_CLAMP_TO_BORDER = 0x812d;
    public const uint GL_CLAMP_TO_EDGE = 0x812f;
    public const uint GL_CLEAR = 0x1500;
    public const uint GL_CLEAR_BUFFER = 0x82b4;
    public const uint GL_CLEAR_TEXTURE = 0x9365;
    public const uint GL_CLIENT_ATTRIB_STACK_DEPTH = 0xbb1;
    public const uint GL_CLIENT_MAPPED_BUFFER_BARRIER_BIT = 0x4000;
    public const uint GL_CLIENT_PIXEL_STORE_BIT = 0x1;
    public const uint GL_CLIENT_STORAGE_BIT = 0x200;
    public const uint GL_CLIENT_VERTEX_ARRAY_BIT = 0x2;
    public const uint GL_CLIPPING_INPUT_PRIMITIVES_ARB = 0x82f6;
    public const uint GL_CLIPPING_OUTPUT_PRIMITIVES_ARB = 0x82f7;
    public const uint GL_CLIP_DEPTH_MODE = 0x935d;
    public const uint GL_CLIP_DISTANCE0 = 0x3000;
    public const uint GL_CLIP_DISTANCE1 = 0x3001;
    public const uint GL_CLIP_DISTANCE2 = 0x3002;
    public const uint GL_CLIP_DISTANCE3 = 0x3003;
    public const uint GL_CLIP_DISTANCE4 = 0x3004;
    public const uint GL_CLIP_DISTANCE5 = 0x3005;
    public const uint GL_CLIP_DISTANCE6 = 0x3006;
    public const uint GL_CLIP_DISTANCE7 = 0x3007;
    public const uint GL_CLIP_ORIGIN = 0x935c;
    public const uint GL_CLIP_PLANE0 = 0x3000;
    public const uint GL_CLIP_PLANE1 = 0x3001;
    public const uint GL_CLIP_PLANE2 = 0x3002;
    public const uint GL_CLIP_PLANE3 = 0x3003;
    public const uint GL_CLIP_PLANE4 = 0x3004;
    public const uint GL_CLIP_PLANE5 = 0x3005;
    public const uint GL_COEFF = 0xa00;
    public const uint GL_COLOR = 0x1800;
    public const uint GL_COLOR_ARRAY = 0x8076;
    public const uint GL_COLOR_ARRAY_COUNT_EXT = 0x8084;
    public const uint GL_COLOR_ARRAY_EXT = 0x8076;
    public const uint GL_COLOR_ARRAY_POINTER = 0x8090;
    public const uint GL_COLOR_ARRAY_POINTER_EXT = 0x8090;
    public const uint GL_COLOR_ARRAY_SIZE = 0x8081;
    public const uint GL_COLOR_ARRAY_SIZE_EXT = 0x8081;
    public const uint GL_COLOR_ARRAY_STRIDE = 0x8083;
    public const uint GL_COLOR_ARRAY_STRIDE_EXT = 0x8083;
    public const uint GL_COLOR_ARRAY_TYPE = 0x8082;
    public const uint GL_COLOR_ARRAY_TYPE_EXT = 0x8082;
    public const uint GL_COLOR_ATTACHMENT0 = 0x8ce0;
    public const uint GL_COLOR_ATTACHMENT1 = 0x8ce1;
    public const uint GL_COLOR_ATTACHMENT10 = 0x8cea;
    public const uint GL_COLOR_ATTACHMENT11 = 0x8ceb;
    public const uint GL_COLOR_ATTACHMENT12 = 0x8cec;
    public const uint GL_COLOR_ATTACHMENT13 = 0x8ced;
    public const uint GL_COLOR_ATTACHMENT14 = 0x8cee;
    public const uint GL_COLOR_ATTACHMENT15 = 0x8cef;
    public const uint GL_COLOR_ATTACHMENT16 = 0x8cf0;
    public const uint GL_COLOR_ATTACHMENT17 = 0x8cf1;
    public const uint GL_COLOR_ATTACHMENT18 = 0x8cf2;
    public const uint GL_COLOR_ATTACHMENT19 = 0x8cf3;
    public const uint GL_COLOR_ATTACHMENT2 = 0x8ce2;
    public const uint GL_COLOR_ATTACHMENT20 = 0x8cf4;
    public const uint GL_COLOR_ATTACHMENT21 = 0x8cf5;
    public const uint GL_COLOR_ATTACHMENT22 = 0x8cf6;
    public const uint GL_COLOR_ATTACHMENT23 = 0x8cf7;
    public const uint GL_COLOR_ATTACHMENT24 = 0x8cf8;
    public const uint GL_COLOR_ATTACHMENT25 = 0x8cf9;
    public const uint GL_COLOR_ATTACHMENT26 = 0x8cfa;
    public const uint GL_COLOR_ATTACHMENT27 = 0x8cfb;
    public const uint GL_COLOR_ATTACHMENT28 = 0x8cfc;
    public const uint GL_COLOR_ATTACHMENT29 = 0x8cfd;
    public const uint GL_COLOR_ATTACHMENT3 = 0x8ce3;
    public const uint GL_COLOR_ATTACHMENT30 = 0x8cfe;
    public const uint GL_COLOR_ATTACHMENT31 = 0x8cff;
    public const uint GL_COLOR_ATTACHMENT4 = 0x8ce4;
    public const uint GL_COLOR_ATTACHMENT5 = 0x8ce5;
    public const uint GL_COLOR_ATTACHMENT6 = 0x8ce6;
    public const uint GL_COLOR_ATTACHMENT7 = 0x8ce7;
    public const uint GL_COLOR_ATTACHMENT8 = 0x8ce8;
    public const uint GL_COLOR_ATTACHMENT9 = 0x8ce9;
    public const uint GL_COLOR_BUFFER_BIT = 0x4000;
    public const uint GL_COLOR_CLEAR_VALUE = 0xc22;
    public const uint GL_COLOR_COMPONENTS = 0x8283;
    public const uint GL_COLOR_ENCODING = 0x8296;
    public const uint GL_COLOR_INDEX = 0x1900;
    public const uint GL_COLOR_INDEX12_EXT = 0x80e6;
    public const uint GL_COLOR_INDEX16_EXT = 0x80e7;
    public const uint GL_COLOR_INDEX1_EXT = 0x80e2;
    public const uint GL_COLOR_INDEX2_EXT = 0x80e3;
    public const uint GL_COLOR_INDEX4_EXT = 0x80e4;
    public const uint GL_COLOR_INDEX8_EXT = 0x80e5;
    public const uint GL_COLOR_INDEXES = 0x1603;
    public const uint GL_COLOR_LOGIC_OP = 0xbf2;
    public const uint GL_COLOR_MATERIAL = 0xb57;
    public const uint GL_COLOR_MATERIAL_FACE = 0xb55;
    public const uint GL_COLOR_MATERIAL_PARAMETER = 0xb56;
    public const uint GL_COLOR_RENDERABLE = 0x8286;
    public const uint GL_COLOR_TABLE_ALPHA_SIZE_EXT = 0x80dd;
    public const uint GL_COLOR_TABLE_BLUE_SIZE_EXT = 0x80dc;
    public const uint GL_COLOR_TABLE_FORMAT_EXT = 0x80d8;
    public const uint GL_COLOR_TABLE_GREEN_SIZE_EXT = 0x80db;
    public const uint GL_COLOR_TABLE_INTENSITY_SIZE_EXT = 0x80df;
    public const uint GL_COLOR_TABLE_LUMINANCE_SIZE_EXT = 0x80de;
    public const uint GL_COLOR_TABLE_RED_SIZE_EXT = 0x80da;
    public const uint GL_COLOR_TABLE_WIDTH_EXT = 0x80d9;
    public const uint GL_COLOR_WRITEMASK = 0xc23;
    public const uint GL_COMMAND_BARRIER_BIT = 0x40;
    public const uint GL_COMPARE_REF_TO_TEXTURE = 0x884e;
    public const uint GL_COMPATIBLE_SUBROUTINES = 0x8e4b;
    public const uint GL_COMPILE = 0x1300;
    public const uint GL_COMPILE_AND_EXECUTE = 0x1301;
    public const uint GL_COMPILE_STATUS = 0x8b81;
    public const uint GL_COMPRESSED_R11_EAC = 0x9270;
    public const uint GL_COMPRESSED_RED = 0x8225;
    public const uint GL_COMPRESSED_RED_RGTC1 = 0x8dbb;
    public const uint GL_COMPRESSED_RG = 0x8226;
    public const uint GL_COMPRESSED_RG11_EAC = 0x9272;
    public const uint GL_COMPRESSED_RGB = 0x84ed;
    public const uint GL_COMPRESSED_RGB8_ETC2 = 0x9274;
    public const uint GL_COMPRESSED_RGB8_PUNCHTHROUGH_ALPHA1_ETC2 = 0x9276;
    public const uint GL_COMPRESSED_RGBA = 0x84ee;
    public const uint GL_COMPRESSED_RGBA8_ETC2_EAC = 0x9278;
    public const uint GL_COMPRESSED_RGBA_BPTC_UNORM = 0x8e8c;
    public const uint GL_COMPRESSED_RGBA_BPTC_UNORM_ARB = 0x8e8c;
    public const uint GL_COMPRESSED_RGB_BPTC_SIGNED_FLOAT = 0x8e8e;
    public const uint GL_COMPRESSED_RGB_BPTC_SIGNED_FLOAT_ARB = 0x8e8e;
    public const uint GL_COMPRESSED_RGB_BPTC_UNSIGNED_FLOAT = 0x8e8f;
    public const uint GL_COMPRESSED_RGB_BPTC_UNSIGNED_FLOAT_ARB = 0x8e8f;
    public const uint GL_COMPRESSED_RG_RGTC2 = 0x8dbd;
    public const uint GL_COMPRESSED_SIGNED_R11_EAC = 0x9271;
    public const uint GL_COMPRESSED_SIGNED_RED_RGTC1 = 0x8dbc;
    public const uint GL_COMPRESSED_SIGNED_RG11_EAC = 0x9273;
    public const uint GL_COMPRESSED_SIGNED_RG_RGTC2 = 0x8dbe;
    public const uint GL_COMPRESSED_SRGB = 0x8c48;
    public const uint GL_COMPRESSED_SRGB8_ALPHA8_ETC2_EAC = 0x9279;
    public const uint GL_COMPRESSED_SRGB8_ETC2 = 0x9275;
    public const uint GL_COMPRESSED_SRGB8_PUNCHTHROUGH_ALPHA1_ETC2 = 0x9277;
    public const uint GL_COMPRESSED_SRGB_ALPHA = 0x8c49;
    public const uint GL_COMPRESSED_SRGB_ALPHA_BPTC_UNORM = 0x8e8d;
    public const uint GL_COMPRESSED_SRGB_ALPHA_BPTC_UNORM_ARB = 0x8e8d;
    public const uint GL_COMPRESSED_TEXTURE_FORMATS = 0x86a3;
    public const uint GL_COMPUTE_SHADER = 0x91b9;
    public const uint GL_COMPUTE_SHADER_BIT = 0x20;
    public const uint GL_COMPUTE_SHADER_INVOCATIONS_ARB = 0x82f5;
    public const uint GL_COMPUTE_SUBROUTINE = 0x92ed;
    public const uint GL_COMPUTE_SUBROUTINE_UNIFORM = 0x92f3;
    public const uint GL_COMPUTE_TEXTURE = 0x82a0;
    public const uint GL_COMPUTE_WORK_GROUP_SIZE = 0x8267;
    public const uint GL_CONDITION_SATISFIED = 0x911c;
    public const uint GL_CONSTANT_ALPHA = 0x8003;
    public const uint GL_CONSTANT_ATTENUATION = 0x1207;
    public const uint GL_CONSTANT_COLOR = 0x8001;
    public const uint GL_CONTEXT_COMPATIBILITY_PROFILE_BIT = 0x2;
    public const uint GL_CONTEXT_CORE_PROFILE_BIT = 0x1;
    public const uint GL_CONTEXT_FLAGS = 0x821e;
    public const uint GL_CONTEXT_FLAG_DEBUG_BIT = 0x2;
    public const uint GL_CONTEXT_FLAG_FORWARD_COMPATIBLE_BIT = 0x1;
    public const uint GL_CONTEXT_FLAG_NO_ERROR_BIT_KHR = 0x8;
    public const uint GL_CONTEXT_FLAG_ROBUST_ACCESS_BIT = 0x4;
    public const uint GL_CONTEXT_FLAG_ROBUST_ACCESS_BIT_ARB = 0x4;
    public const uint GL_CONTEXT_LOST = 0x507;
    public const uint GL_CONTEXT_PROFILE_MASK = 0x9126;
    public const uint GL_CONTEXT_RELEASE_BEHAVIOR = 0x82fb;
    public const uint GL_CONTEXT_RELEASE_BEHAVIOR_FLUSH = 0x82fc;
    public const uint GL_CONTEXT_ROBUST_ACCESS = 0x90f3;
    public const uint GL_COPY = 0x1503;
    public const uint GL_COPY_INVERTED = 0x150c;
    public const uint GL_COPY_PIXEL_TOKEN = 0x706;
    public const uint GL_COPY_READ_BUFFER = 0x8f36;
    public const uint GL_COPY_READ_BUFFER_BINDING = 0x8f36;
    public const uint GL_COPY_WRITE_BUFFER = 0x8f37;
    public const uint GL_COPY_WRITE_BUFFER_BINDING = 0x8f37;
    public const uint GL_CULL_FACE = 0xb44;
    public const uint GL_CULL_FACE_MODE = 0xb45;
    public const uint GL_CURRENT_BIT = 0x1;
    public const uint GL_CURRENT_COLOR = 0xb00;
    public const uint GL_CURRENT_INDEX = 0xb01;
    public const uint GL_CURRENT_NORMAL = 0xb02;
    public const uint GL_CURRENT_PROGRAM = 0x8b8d;
    public const uint GL_CURRENT_QUERY = 0x8865;
    public const uint GL_CURRENT_RASTER_COLOR = 0xb04;
    public const uint GL_CURRENT_RASTER_DISTANCE = 0xb09;
    public const uint GL_CURRENT_RASTER_INDEX = 0xb05;
    public const uint GL_CURRENT_RASTER_POSITION = 0xb07;
    public const uint GL_CURRENT_RASTER_POSITION_VALID = 0xb08;
    public const uint GL_CURRENT_RASTER_TEXTURE_COORDS = 0xb06;
    public const uint GL_CURRENT_TEXTURE_COORDS = 0xb03;
    public const uint GL_CURRENT_VERTEX_ATTRIB = 0x8626;
    public const uint GL_CW = 0x900;
    public const uint GL_DEBUG_CALLBACK_FUNCTION = 0x8244;
    public const uint GL_DEBUG_CALLBACK_FUNCTION_ARB = 0x8244;
    public const uint GL_DEBUG_CALLBACK_USER_PARAM = 0x8245;
    public const uint GL_DEBUG_CALLBACK_USER_PARAM_ARB = 0x8245;
    public const uint GL_DEBUG_GROUP_STACK_DEPTH = 0x826d;
    public const uint GL_DEBUG_LOGGED_MESSAGES = 0x9145;
    public const uint GL_DEBUG_LOGGED_MESSAGES_ARB = 0x9145;
    public const uint GL_DEBUG_NEXT_LOGGED_MESSAGE_LENGTH = 0x8243;
    public const uint GL_DEBUG_NEXT_LOGGED_MESSAGE_LENGTH_ARB = 0x8243;
    public const uint GL_DEBUG_OUTPUT = 0x92e0;
    public const uint GL_DEBUG_OUTPUT_SYNCHRONOUS = 0x8242;
    public const uint GL_DEBUG_OUTPUT_SYNCHRONOUS_ARB = 0x8242;
    public const uint GL_DEBUG_SEVERITY_HIGH = 0x9146;
    public const uint GL_DEBUG_SEVERITY_HIGH_ARB = 0x9146;
    public const uint GL_DEBUG_SEVERITY_LOW = 0x9148;
    public const uint GL_DEBUG_SEVERITY_LOW_ARB = 0x9148;
    public const uint GL_DEBUG_SEVERITY_MEDIUM = 0x9147;
    public const uint GL_DEBUG_SEVERITY_MEDIUM_ARB = 0x9147;
    public const uint GL_DEBUG_SEVERITY_NOTIFICATION = 0x826b;
    public const uint GL_DEBUG_SOURCE_API = 0x8246;
    public const uint GL_DEBUG_SOURCE_API_ARB = 0x8246;
    public const uint GL_DEBUG_SOURCE_APPLICATION = 0x824a;
    public const uint GL_DEBUG_SOURCE_APPLICATION_ARB = 0x824a;
    public const uint GL_DEBUG_SOURCE_OTHER = 0x824b;
    public const uint GL_DEBUG_SOURCE_OTHER_ARB = 0x824b;
    public const uint GL_DEBUG_SOURCE_SHADER_COMPILER = 0x8248;
    public const uint GL_DEBUG_SOURCE_SHADER_COMPILER_ARB = 0x8248;
    public const uint GL_DEBUG_SOURCE_THIRD_PARTY = 0x8249;
    public const uint GL_DEBUG_SOURCE_THIRD_PARTY_ARB = 0x8249;
    public const uint GL_DEBUG_SOURCE_WINDOW_SYSTEM = 0x8247;
    public const uint GL_DEBUG_SOURCE_WINDOW_SYSTEM_ARB = 0x8247;
    public const uint GL_DEBUG_TYPE_DEPRECATED_BEHAVIOR = 0x824d;
    public const uint GL_DEBUG_TYPE_DEPRECATED_BEHAVIOR_ARB = 0x824d;
    public const uint GL_DEBUG_TYPE_ERROR = 0x824c;
    public const uint GL_DEBUG_TYPE_ERROR_ARB = 0x824c;
    public const uint GL_DEBUG_TYPE_MARKER = 0x8268;
    public const uint GL_DEBUG_TYPE_OTHER = 0x8251;
    public const uint GL_DEBUG_TYPE_OTHER_ARB = 0x8251;
    public const uint GL_DEBUG_TYPE_PERFORMANCE = 0x8250;
    public const uint GL_DEBUG_TYPE_PERFORMANCE_ARB = 0x8250;
    public const uint GL_DEBUG_TYPE_POP_GROUP = 0x826a;
    public const uint GL_DEBUG_TYPE_PORTABILITY = 0x824f;
    public const uint GL_DEBUG_TYPE_PORTABILITY_ARB = 0x824f;
    public const uint GL_DEBUG_TYPE_PUSH_GROUP = 0x8269;
    public const uint GL_DEBUG_TYPE_UNDEFINED_BEHAVIOR = 0x824e;
    public const uint GL_DEBUG_TYPE_UNDEFINED_BEHAVIOR_ARB = 0x824e;
    public const uint GL_DECAL = 0x2101;
    public const uint GL_DECR = 0x1e03;
    public const uint GL_DECR_WRAP = 0x8508;
    public const uint GL_DELETE_STATUS = 0x8b80;
    public const uint GL_DEPTH = 0x1801;
    public const uint GL_DEPTH24_STENCIL8 = 0x88f0;
    public const uint GL_DEPTH32F_STENCIL8 = 0x8cad;
    public const uint GL_DEPTH_ATTACHMENT = 0x8d00;
    public const uint GL_DEPTH_BIAS = 0xd1f;
    public const uint GL_DEPTH_BITS = 0xd56;
    public const uint GL_DEPTH_BUFFER_BIT = 0x100;
    public const uint GL_DEPTH_CLAMP = 0x864f;
    public const uint GL_DEPTH_CLEAR_VALUE = 0xb73;
    public const uint GL_DEPTH_COMPONENT = 0x1902;
    public const uint GL_DEPTH_COMPONENT16 = 0x81a5;
    public const uint GL_DEPTH_COMPONENT24 = 0x81a6;
    public const uint GL_DEPTH_COMPONENT32 = 0x81a7;
    public const uint GL_DEPTH_COMPONENT32F = 0x8cac;
    public const uint GL_DEPTH_COMPONENTS = 0x8284;
    public const uint GL_DEPTH_FUNC = 0xb74;
    public const uint GL_DEPTH_RANGE = 0xb70;
    public const uint GL_DEPTH_RENDERABLE = 0x8287;
    public const uint GL_DEPTH_SCALE = 0xd1e;
    public const uint GL_DEPTH_STENCIL = 0x84f9;
    public const uint GL_DEPTH_STENCIL_ATTACHMENT = 0x821a;
    public const uint GL_DEPTH_STENCIL_TEXTURE_MODE = 0x90ea;
    public const uint GL_DEPTH_TEST = 0xb71;
    public const uint GL_DEPTH_WRITEMASK = 0xb72;
    public const uint GL_DIFFUSE = 0x1201;
    public const uint GL_DISPATCH_INDIRECT_BUFFER = 0x90ee;
    public const uint GL_DISPATCH_INDIRECT_BUFFER_BINDING = 0x90ef;
    public const uint GL_DITHER = 0xbd0;
    public const uint GL_DOMAIN = 0xa02;
    public const uint GL_DONT_CARE = 0x1100;
    public const uint GL_DOUBLE = 0x140a;
    public const uint GL_DOUBLEBUFFER = 0xc32;
    public const uint GL_DOUBLE_MAT2 = 0x8f46;
    public const uint GL_DOUBLE_MAT3 = 0x8f47;
    public const uint GL_DOUBLE_MAT4 = 0x8f48;
    public const uint GL_DOUBLE_VEC2 = 0x8ffc;
    public const uint GL_DOUBLE_VEC3 = 0x8ffd;
    public const uint GL_DOUBLE_VEC4 = 0x8ffe;
    public const uint GL_DRAW_BUFFER = 0xc01;
    public const uint GL_DRAW_BUFFER0 = 0x8825;
    public const uint GL_DRAW_BUFFER1 = 0x8826;
    public const uint GL_DRAW_BUFFER10 = 0x882f;
    public const uint GL_DRAW_BUFFER11 = 0x8830;
    public const uint GL_DRAW_BUFFER12 = 0x8831;
    public const uint GL_DRAW_BUFFER13 = 0x8832;
    public const uint GL_DRAW_BUFFER14 = 0x8833;
    public const uint GL_DRAW_BUFFER15 = 0x8834;
    public const uint GL_DRAW_BUFFER2 = 0x8827;
    public const uint GL_DRAW_BUFFER3 = 0x8828;
    public const uint GL_DRAW_BUFFER4 = 0x8829;
    public const uint GL_DRAW_BUFFER5 = 0x882a;
    public const uint GL_DRAW_BUFFER6 = 0x882b;
    public const uint GL_DRAW_BUFFER7 = 0x882c;
    public const uint GL_DRAW_BUFFER8 = 0x882d;
    public const uint GL_DRAW_BUFFER9 = 0x882e;
    public const uint GL_DRAW_FRAMEBUFFER = 0x8ca9;
    public const uint GL_DRAW_FRAMEBUFFER_BINDING = 0x8ca6;
    public const uint GL_DRAW_INDIRECT_BUFFER = 0x8f3f;
    public const uint GL_DRAW_INDIRECT_BUFFER_BINDING = 0x8f43;
    public const uint GL_DRAW_PIXEL_TOKEN = 0x705;
    public const uint GL_DST_ALPHA = 0x304;
    public const uint GL_DST_COLOR = 0x306;
    public const uint GL_DYNAMIC_COPY = 0x88ea;
    public const uint GL_DYNAMIC_DRAW = 0x88e8;
    public const uint GL_DYNAMIC_READ = 0x88e9;
    public const uint GL_DYNAMIC_STORAGE_BIT = 0x100;
    public const uint GL_EDGE_FLAG = 0xb43;
    public const uint GL_EDGE_FLAG_ARRAY = 0x8079;
    public const uint GL_EDGE_FLAG_ARRAY_COUNT_EXT = 0x808d;
    public const uint GL_EDGE_FLAG_ARRAY_EXT = 0x8079;
    public const uint GL_EDGE_FLAG_ARRAY_POINTER = 0x8093;
    public const uint GL_EDGE_FLAG_ARRAY_POINTER_EXT = 0x8093;
    public const uint GL_EDGE_FLAG_ARRAY_STRIDE = 0x808c;
    public const uint GL_EDGE_FLAG_ARRAY_STRIDE_EXT = 0x808c;
    public const uint GL_ELEMENT_ARRAY_BARRIER_BIT = 0x2;
    public const uint GL_ELEMENT_ARRAY_BUFFER = 0x8893;
    public const uint GL_ELEMENT_ARRAY_BUFFER_BINDING = 0x8895;
    public const uint GL_EMISSION = 0x1600;
    public const uint GL_ENABLE_BIT = 0x2000;
    public const uint GL_EQUAL = 0x202;
    public const uint GL_EQUIV = 0x1509;
    public const uint GL_EVAL_BIT = 0x10000;
    public const uint GL_EXP = 0x800;
    public const uint GL_EXP2 = 0x801;
    public const uint GL_EXTENSIONS = 0x1f03;
    public const uint GL_EYE_LINEAR = 0x2400;
    public const uint GL_EYE_PLANE = 0x2502;
    public const uint GL_FALSE = 0x0;
    public const uint GL_FASTEST = 0x1101;
    public const uint GL_FEEDBACK = 0x1c01;
    public const uint GL_FEEDBACK_BUFFER_POINTER = 0xdf0;
    public const uint GL_FEEDBACK_BUFFER_SIZE = 0xdf1;
    public const uint GL_FEEDBACK_BUFFER_TYPE = 0xdf2;
    public const uint GL_FILL = 0x1b02;
    public const uint GL_FILTER = 0x829a;
    public const uint GL_FIRST_VERTEX_CONVENTION = 0x8e4d;
    public const uint GL_FIXED = 0x140c;
    public const uint GL_FIXED_ONLY = 0x891d;
    public const uint GL_FLAT = 0x1d00;
    public const uint GL_FLOAT = 0x1406;
    public const uint GL_FLOAT_32_UNSIGNED_INT_24_8_REV = 0x8dad;
    public const uint GL_FLOAT_MAT2 = 0x8b5a;
    public const uint GL_FLOAT_MAT3 = 0x8b5b;
    public const uint GL_FLOAT_MAT4 = 0x8b5c;
    public const uint GL_FLOAT_VEC2 = 0x8b50;
    public const uint GL_FLOAT_VEC3 = 0x8b51;
    public const uint GL_FLOAT_VEC4 = 0x8b52;
    public const uint GL_FOG = 0xb60;
    public const uint GL_FOG_BIT = 0x80;
    public const uint GL_FOG_COLOR = 0xb66;
    public const uint GL_FOG_DENSITY = 0xb62;
    public const uint GL_FOG_END = 0xb64;
    public const uint GL_FOG_HINT = 0xc54;
    public const uint GL_FOG_INDEX = 0xb61;
    public const uint GL_FOG_MODE = 0xb65;
    public const uint GL_FOG_SPECULAR_TEXTURE_WIN = 0x80ec;
    public const uint GL_FOG_START = 0xb63;
    public const uint GL_FRACTIONAL_EVEN = 0x8e7c;
    public const uint GL_FRACTIONAL_ODD = 0x8e7b;
    public const uint GL_FRAGMENT_INTERPOLATION_OFFSET_BITS = 0x8e5d;
    public const uint GL_FRAGMENT_SHADER = 0x8b30;
    public const uint GL_FRAGMENT_SHADER_BIT = 0x2;
    public const uint GL_FRAGMENT_SHADER_DERIVATIVE_HINT = 0x8b8b;
    public const uint GL_FRAGMENT_SHADER_INVOCATIONS_ARB = 0x82f4;
    public const uint GL_FRAGMENT_SUBROUTINE = 0x92ec;
    public const uint GL_FRAGMENT_SUBROUTINE_UNIFORM = 0x92f2;
    public const uint GL_FRAGMENT_TEXTURE = 0x829f;
    public const uint GL_FRAMEBUFFER = 0x8d40;
    public const uint GL_FRAMEBUFFER_ATTACHMENT_ALPHA_SIZE = 0x8215;
    public const uint GL_FRAMEBUFFER_ATTACHMENT_BLUE_SIZE = 0x8214;
    public const uint GL_FRAMEBUFFER_ATTACHMENT_COLOR_ENCODING = 0x8210;
    public const uint GL_FRAMEBUFFER_ATTACHMENT_COMPONENT_TYPE = 0x8211;
    public const uint GL_FRAMEBUFFER_ATTACHMENT_DEPTH_SIZE = 0x8216;
    public const uint GL_FRAMEBUFFER_ATTACHMENT_GREEN_SIZE = 0x8213;
    public const uint GL_FRAMEBUFFER_ATTACHMENT_LAYERED = 0x8da7;
    public const uint GL_FRAMEBUFFER_ATTACHMENT_OBJECT_NAME = 0x8cd1;
    public const uint GL_FRAMEBUFFER_ATTACHMENT_OBJECT_TYPE = 0x8cd0;
    public const uint GL_FRAMEBUFFER_ATTACHMENT_RED_SIZE = 0x8212;
    public const uint GL_FRAMEBUFFER_ATTACHMENT_STENCIL_SIZE = 0x8217;
    public const uint GL_FRAMEBUFFER_ATTACHMENT_TEXTURE_CUBE_MAP_FACE = 0x8cd3;
    public const uint GL_FRAMEBUFFER_ATTACHMENT_TEXTURE_LAYER = 0x8cd4;
    public const uint GL_FRAMEBUFFER_ATTACHMENT_TEXTURE_LEVEL = 0x8cd2;
    public const uint GL_FRAMEBUFFER_BARRIER_BIT = 0x400;
    public const uint GL_FRAMEBUFFER_BINDING = 0x8ca6;
    public const uint GL_FRAMEBUFFER_BLEND = 0x828b;
    public const uint GL_FRAMEBUFFER_COMPLETE = 0x8cd5;
    public const uint GL_FRAMEBUFFER_DEFAULT = 0x8218;
    public const uint GL_FRAMEBUFFER_DEFAULT_FIXED_SAMPLE_LOCATIONS = 0x9314;
    public const uint GL_FRAMEBUFFER_DEFAULT_HEIGHT = 0x9311;
    public const uint GL_FRAMEBUFFER_DEFAULT_LAYERS = 0x9312;
    public const uint GL_FRAMEBUFFER_DEFAULT_SAMPLES = 0x9313;
    public const uint GL_FRAMEBUFFER_DEFAULT_WIDTH = 0x9310;
    public const uint GL_FRAMEBUFFER_INCOMPLETE_ATTACHMENT = 0x8cd6;
    public const uint GL_FRAMEBUFFER_INCOMPLETE_DRAW_BUFFER = 0x8cdb;
    public const uint GL_FRAMEBUFFER_INCOMPLETE_LAYER_TARGETS = 0x8da8;
    public const uint GL_FRAMEBUFFER_INCOMPLETE_MISSING_ATTACHMENT = 0x8cd7;
    public const uint GL_FRAMEBUFFER_INCOMPLETE_MULTISAMPLE = 0x8d56;
    public const uint GL_FRAMEBUFFER_INCOMPLETE_READ_BUFFER = 0x8cdc;
    public const uint GL_FRAMEBUFFER_RENDERABLE = 0x8289;
    public const uint GL_FRAMEBUFFER_RENDERABLE_LAYERED = 0x828a;
    public const uint GL_FRAMEBUFFER_SRGB = 0x8db9;
    public const uint GL_FRAMEBUFFER_UNDEFINED = 0x8219;
    public const uint GL_FRAMEBUFFER_UNSUPPORTED = 0x8cdd;
    public const uint GL_FRONT = 0x404;
    public const uint GL_FRONT_AND_BACK = 0x408;
    public const uint GL_FRONT_FACE = 0xb46;
    public const uint GL_FRONT_LEFT = 0x400;
    public const uint GL_FRONT_RIGHT = 0x401;
    public const uint GL_FULL_SUPPORT = 0x82b7;
    public const uint GL_FUNC_ADD = 0x8006;
    public const uint GL_FUNC_REVERSE_SUBTRACT = 0x800b;
    public const uint GL_FUNC_SUBTRACT = 0x800a;
    public const uint GL_GEOMETRY_INPUT_TYPE = 0x8917;
    public const uint GL_GEOMETRY_OUTPUT_TYPE = 0x8918;
    public const uint GL_GEOMETRY_SHADER = 0x8dd9;
    public const uint GL_GEOMETRY_SHADER_BIT = 0x4;
    public const uint GL_GEOMETRY_SHADER_INVOCATIONS = 0x887f;
    public const uint GL_GEOMETRY_SHADER_PRIMITIVES_EMITTED_ARB = 0x82f3;
    public const uint GL_GEOMETRY_SUBROUTINE = 0x92eb;
    public const uint GL_GEOMETRY_SUBROUTINE_UNIFORM = 0x92f1;
    public const uint GL_GEOMETRY_TEXTURE = 0x829e;
    public const uint GL_GEOMETRY_VERTICES_OUT = 0x8916;
    public const uint GL_GEQUAL = 0x206;
    public const uint GL_GET_TEXTURE_IMAGE_FORMAT = 0x8291;
    public const uint GL_GET_TEXTURE_IMAGE_TYPE = 0x8292;
    public const uint GL_GREATER = 0x204;
    public const uint GL_GREEN = 0x1904;
    public const uint GL_GREEN_BIAS = 0xd19;
    public const uint GL_GREEN_BITS = 0xd53;
    public const uint GL_GREEN_INTEGER = 0x8d95;
    public const uint GL_GREEN_SCALE = 0xd18;
    public const uint GL_GUILTY_CONTEXT_RESET = 0x8253;
    public const uint GL_GUILTY_CONTEXT_RESET_ARB = 0x8253;
    public const uint GL_HALF_FLOAT = 0x140b;
    public const uint GL_HIGH_FLOAT = 0x8df2;
    public const uint GL_HIGH_INT = 0x8df5;
    public const uint GL_HINT_BIT = 0x8000;
    public const uint GL_IMAGE_1D = 0x904c;
    public const uint GL_IMAGE_1D_ARRAY = 0x9052;
    public const uint GL_IMAGE_2D = 0x904d;
    public const uint GL_IMAGE_2D_ARRAY = 0x9053;
    public const uint GL_IMAGE_2D_MULTISAMPLE = 0x9055;
    public const uint GL_IMAGE_2D_MULTISAMPLE_ARRAY = 0x9056;
    public const uint GL_IMAGE_2D_RECT = 0x904f;
    public const uint GL_IMAGE_3D = 0x904e;
    public const uint GL_IMAGE_BINDING_ACCESS = 0x8f3e;
    public const uint GL_IMAGE_BINDING_FORMAT = 0x906e;
    public const uint GL_IMAGE_BINDING_LAYER = 0x8f3d;
    public const uint GL_IMAGE_BINDING_LAYERED = 0x8f3c;
    public const uint GL_IMAGE_BINDING_LEVEL = 0x8f3b;
    public const uint GL_IMAGE_BINDING_NAME = 0x8f3a;
    public const uint GL_IMAGE_BUFFER = 0x9051;
    public const uint GL_IMAGE_CLASS_10_10_10_2 = 0x82c3;
    public const uint GL_IMAGE_CLASS_11_11_10 = 0x82c2;
    public const uint GL_IMAGE_CLASS_1_X_16 = 0x82be;
    public const uint GL_IMAGE_CLASS_1_X_32 = 0x82bb;
    public const uint GL_IMAGE_CLASS_1_X_8 = 0x82c1;
    public const uint GL_IMAGE_CLASS_2_X_16 = 0x82bd;
    public const uint GL_IMAGE_CLASS_2_X_32 = 0x82ba;
    public const uint GL_IMAGE_CLASS_2_X_8 = 0x82c0;
    public const uint GL_IMAGE_CLASS_4_X_16 = 0x82bc;
    public const uint GL_IMAGE_CLASS_4_X_32 = 0x82b9;
    public const uint GL_IMAGE_CLASS_4_X_8 = 0x82bf;
    public const uint GL_IMAGE_COMPATIBILITY_CLASS = 0x82a8;
    public const uint GL_IMAGE_CUBE = 0x9050;
    public const uint GL_IMAGE_CUBE_MAP_ARRAY = 0x9054;
    public const uint GL_IMAGE_FORMAT_COMPATIBILITY_BY_CLASS = 0x90c9;
    public const uint GL_IMAGE_FORMAT_COMPATIBILITY_BY_SIZE = 0x90c8;
    public const uint GL_IMAGE_FORMAT_COMPATIBILITY_TYPE = 0x90c7;
    public const uint GL_IMAGE_PIXEL_FORMAT = 0x82a9;
    public const uint GL_IMAGE_PIXEL_TYPE = 0x82aa;
    public const uint GL_IMAGE_TEXEL_SIZE = 0x82a7;
    public const uint GL_IMPLEMENTATION_COLOR_READ_FORMAT = 0x8b9b;
    public const uint GL_IMPLEMENTATION_COLOR_READ_TYPE = 0x8b9a;
    public const uint GL_INCR = 0x1e02;
    public const uint GL_INCR_WRAP = 0x8507;
    public const uint GL_INDEX_ARRAY = 0x8077;
    public const uint GL_INDEX_ARRAY_COUNT_EXT = 0x8087;
    public const uint GL_INDEX_ARRAY_EXT = 0x8077;
    public const uint GL_INDEX_ARRAY_POINTER = 0x8091;
    public const uint GL_INDEX_ARRAY_POINTER_EXT = 0x8091;
    public const uint GL_INDEX_ARRAY_STRIDE = 0x8086;
    public const uint GL_INDEX_ARRAY_STRIDE_EXT = 0x8086;
    public const uint GL_INDEX_ARRAY_TYPE = 0x8085;
    public const uint GL_INDEX_ARRAY_TYPE_EXT = 0x8085;
    public const uint GL_INDEX_BITS = 0xd51;
    public const uint GL_INDEX_CLEAR_VALUE = 0xc20;
    public const uint GL_INDEX_LOGIC_OP = 0xbf1;
    public const uint GL_INDEX_MODE = 0xc30;
    public const uint GL_INDEX_OFFSET = 0xd13;
    public const uint GL_INDEX_SHIFT = 0xd12;
    public const uint GL_INDEX_WRITEMASK = 0xc21;
    public const uint GL_INFO_LOG_LENGTH = 0x8b84;
    public const uint GL_INNOCENT_CONTEXT_RESET = 0x8254;
    public const uint GL_INNOCENT_CONTEXT_RESET_ARB = 0x8254;
    public const uint GL_INT = 0x1404;
    public const uint GL_INTENSITY = 0x8049;
    public const uint GL_INTENSITY12 = 0x804c;
    public const uint GL_INTENSITY16 = 0x804d;
    public const uint GL_INTENSITY4 = 0x804a;
    public const uint GL_INTENSITY8 = 0x804b;
    public const uint GL_INTERLEAVED_ATTRIBS = 0x8c8c;
    public const uint GL_INTERNALFORMAT_ALPHA_SIZE = 0x8274;
    public const uint GL_INTERNALFORMAT_ALPHA_TYPE = 0x827b;
    public const uint GL_INTERNALFORMAT_BLUE_SIZE = 0x8273;
    public const uint GL_INTERNALFORMAT_BLUE_TYPE = 0x827a;
    public const uint GL_INTERNALFORMAT_DEPTH_SIZE = 0x8275;
    public const uint GL_INTERNALFORMAT_DEPTH_TYPE = 0x827c;
    public const uint GL_INTERNALFORMAT_GREEN_SIZE = 0x8272;
    public const uint GL_INTERNALFORMAT_GREEN_TYPE = 0x8279;
    public const uint GL_INTERNALFORMAT_PREFERRED = 0x8270;
    public const uint GL_INTERNALFORMAT_RED_SIZE = 0x8271;
    public const uint GL_INTERNALFORMAT_RED_TYPE = 0x8278;
    public const uint GL_INTERNALFORMAT_SHARED_SIZE = 0x8277;
    public const uint GL_INTERNALFORMAT_STENCIL_SIZE = 0x8276;
    public const uint GL_INTERNALFORMAT_STENCIL_TYPE = 0x827d;
    public const uint GL_INTERNALFORMAT_SUPPORTED = 0x826f;
    public const uint GL_INT_2_10_10_10_REV = 0x8d9f;
    public const uint GL_INT_IMAGE_1D = 0x9057;
    public const uint GL_INT_IMAGE_1D_ARRAY = 0x905d;
    public const uint GL_INT_IMAGE_2D = 0x9058;
    public const uint GL_INT_IMAGE_2D_ARRAY = 0x905e;
    public const uint GL_INT_IMAGE_2D_MULTISAMPLE = 0x9060;
    public const uint GL_INT_IMAGE_2D_MULTISAMPLE_ARRAY = 0x9061;
    public const uint GL_INT_IMAGE_2D_RECT = 0x905a;
    public const uint GL_INT_IMAGE_3D = 0x9059;
    public const uint GL_INT_IMAGE_BUFFER = 0x905c;
    public const uint GL_INT_IMAGE_CUBE = 0x905b;
    public const uint GL_INT_IMAGE_CUBE_MAP_ARRAY = 0x905f;
    public const uint GL_INT_SAMPLER_1D = 0x8dc9;
    public const uint GL_INT_SAMPLER_1D_ARRAY = 0x8dce;
    public const uint GL_INT_SAMPLER_2D = 0x8dca;
    public const uint GL_INT_SAMPLER_2D_ARRAY = 0x8dcf;
    public const uint GL_INT_SAMPLER_2D_MULTISAMPLE = 0x9109;
    public const uint GL_INT_SAMPLER_2D_MULTISAMPLE_ARRAY = 0x910c;
    public const uint GL_INT_SAMPLER_2D_RECT = 0x8dcd;
    public const uint GL_INT_SAMPLER_3D = 0x8dcb;
    public const uint GL_INT_SAMPLER_BUFFER = 0x8dd0;
    public const uint GL_INT_SAMPLER_CUBE = 0x8dcc;
    public const uint GL_INT_SAMPLER_CUBE_MAP_ARRAY = 0x900e;
    public const uint GL_INT_SAMPLER_CUBE_MAP_ARRAY_ARB = 0x900e;
    public const uint GL_INT_VEC2 = 0x8b53;
    public const uint GL_INT_VEC3 = 0x8b54;
    public const uint GL_INT_VEC4 = 0x8b55;
    public const uint GL_INVALID_ENUM = 0x500;
    public const uint GL_INVALID_FRAMEBUFFER_OPERATION = 0x506;
    public const uint GL_INVALID_OPERATION = 0x502;
    public const uint GL_INVALID_VALUE = 0x501;
    public const uint GL_INVERT = 0x150a;
    public const uint GL_ISOLINES = 0x8e7a;
    public const uint GL_IS_PER_PATCH = 0x92e7;
    public const uint GL_IS_ROW_MAJOR = 0x9300;
    public const uint GL_KEEP = 0x1e00;
    public const uint GL_LAST_VERTEX_CONVENTION = 0x8e4e;
    public const uint GL_LAYER_PROVOKING_VERTEX = 0x825e;
    public const uint GL_LEFT = 0x406;
    public const uint GL_LEQUAL = 0x203;
    public const uint GL_LESS = 0x201;
    public const uint GL_LIGHT0 = 0x4000;
    public const uint GL_LIGHT1 = 0x4001;
    public const uint GL_LIGHT2 = 0x4002;
    public const uint GL_LIGHT3 = 0x4003;
    public const uint GL_LIGHT4 = 0x4004;
    public const uint GL_LIGHT5 = 0x4005;
    public const uint GL_LIGHT6 = 0x4006;
    public const uint GL_LIGHT7 = 0x4007;
    public const uint GL_LIGHTING = 0xb50;
    public const uint GL_LIGHTING_BIT = 0x40;
    public const uint GL_LIGHT_MODEL_AMBIENT = 0xb53;
    public const uint GL_LIGHT_MODEL_LOCAL_VIEWER = 0xb51;
    public const uint GL_LIGHT_MODEL_TWO_SIDE = 0xb52;
    public const uint GL_LINE = 0x1b01;
    public const uint GL_LINEAR = 0x2601;
    public const uint GL_LINEAR_ATTENUATION = 0x1208;
    public const uint GL_LINEAR_MIPMAP_LINEAR = 0x2703;
    public const uint GL_LINEAR_MIPMAP_NEAREST = 0x2701;
    public const uint GL_LINES = 0x1;
    public const uint GL_LINES_ADJACENCY = 0xa;
    public const uint GL_LINE_BIT = 0x4;
    public const uint GL_LINE_LOOP = 0x2;
    public const uint GL_LINE_RESET_TOKEN = 0x707;
    public const uint GL_LINE_SMOOTH = 0xb20;
    public const uint GL_LINE_SMOOTH_HINT = 0xc52;
    public const uint GL_LINE_STIPPLE = 0xb24;
    public const uint GL_LINE_STIPPLE_PATTERN = 0xb25;
    public const uint GL_LINE_STIPPLE_REPEAT = 0xb26;
    public const uint GL_LINE_STRIP = 0x3;
    public const uint GL_LINE_STRIP_ADJACENCY = 0xb;
    public const uint GL_LINE_TOKEN = 0x702;
    public const uint GL_LINE_WIDTH = 0xb21;
    public const uint GL_LINE_WIDTH_GRANULARITY = 0xb23;
    public const uint GL_LINE_WIDTH_RANGE = 0xb22;
    public const uint GL_LINK_STATUS = 0x8b82;
    public const uint GL_LIST_BASE = 0xb32;
    public const uint GL_LIST_BIT = 0x20000;
    public const uint GL_LIST_INDEX = 0xb33;
    public const uint GL_LIST_MODE = 0xb30;
    public const uint GL_LOAD = 0x101;
    public const uint GL_LOCATION = 0x930e;
    public const uint GL_LOCATION_COMPONENT = 0x934a;
    public const uint GL_LOCATION_INDEX = 0x930f;
    public const uint GL_LOGIC_OP_MODE = 0xbf0;
    public const uint GL_LOSE_CONTEXT_ON_RESET = 0x8252;
    public const uint GL_LOSE_CONTEXT_ON_RESET_ARB = 0x8252;
    public const uint GL_LOWER_LEFT = 0x8ca1;
    public const uint GL_LOW_FLOAT = 0x8df0;
    public const uint GL_LOW_INT = 0x8df3;
    public const uint GL_LUMINANCE = 0x1909;
    public const uint GL_LUMINANCE12 = 0x8041;
    public const uint GL_LUMINANCE12_ALPHA12 = 0x8047;
    public const uint GL_LUMINANCE12_ALPHA4 = 0x8046;
    public const uint GL_LUMINANCE16 = 0x8042;
    public const uint GL_LUMINANCE16_ALPHA16 = 0x8048;
    public const uint GL_LUMINANCE4 = 0x803f;
    public const uint GL_LUMINANCE4_ALPHA4 = 0x8043;
    public const uint GL_LUMINANCE6_ALPHA2 = 0x8044;
    public const uint GL_LUMINANCE8 = 0x8040;
    public const uint GL_LUMINANCE8_ALPHA8 = 0x8045;
    public const uint GL_LUMINANCE_ALPHA = 0x190a;
    public const uint GL_MAJOR_VERSION = 0x821b;
    public const uint GL_MANUAL_GENERATE_MIPMAP = 0x8294;
    public const uint GL_MAP1_COLOR_4 = 0xd90;
    public const uint GL_MAP1_GRID_DOMAIN = 0xdd0;
    public const uint GL_MAP1_GRID_SEGMENTS = 0xdd1;
    public const uint GL_MAP1_INDEX = 0xd91;
    public const uint GL_MAP1_NORMAL = 0xd92;
    public const uint GL_MAP1_TEXTURE_COORD_1 = 0xd93;
    public const uint GL_MAP1_TEXTURE_COORD_2 = 0xd94;
    public const uint GL_MAP1_TEXTURE_COORD_3 = 0xd95;
    public const uint GL_MAP1_TEXTURE_COORD_4 = 0xd96;
    public const uint GL_MAP1_VERTEX_3 = 0xd97;
    public const uint GL_MAP1_VERTEX_4 = 0xd98;
    public const uint GL_MAP2_COLOR_4 = 0xdb0;
    public const uint GL_MAP2_GRID_DOMAIN = 0xdd2;
    public const uint GL_MAP2_GRID_SEGMENTS = 0xdd3;
    public const uint GL_MAP2_INDEX = 0xdb1;
    public const uint GL_MAP2_NORMAL = 0xdb2;
    public const uint GL_MAP2_TEXTURE_COORD_1 = 0xdb3;
    public const uint GL_MAP2_TEXTURE_COORD_2 = 0xdb4;
    public const uint GL_MAP2_TEXTURE_COORD_3 = 0xdb5;
    public const uint GL_MAP2_TEXTURE_COORD_4 = 0xdb6;
    public const uint GL_MAP2_VERTEX_3 = 0xdb7;
    public const uint GL_MAP2_VERTEX_4 = 0xdb8;
    public const uint GL_MAP_COHERENT_BIT = 0x80;
    public const uint GL_MAP_COLOR = 0xd10;
    public const uint GL_MAP_FLUSH_EXPLICIT_BIT = 0x10;
    public const uint GL_MAP_INVALIDATE_BUFFER_BIT = 0x8;
    public const uint GL_MAP_INVALIDATE_RANGE_BIT = 0x4;
    public const uint GL_MAP_PERSISTENT_BIT = 0x40;
    public const uint GL_MAP_READ_BIT = 0x1;
    public const uint GL_MAP_STENCIL = 0xd11;
    public const uint GL_MAP_UNSYNCHRONIZED_BIT = 0x20;
    public const uint GL_MAP_WRITE_BIT = 0x2;
    public const uint GL_MATRIX_MODE = 0xba0;
    public const uint GL_MATRIX_STRIDE = 0x92ff;
    public const uint GL_MAX = 0x8008;
    public const uint GL_MAX_3D_TEXTURE_SIZE = 0x8073;
    public const uint GL_MAX_ARRAY_TEXTURE_LAYERS = 0x88ff;
    public const uint GL_MAX_ATOMIC_COUNTER_BUFFER_BINDINGS = 0x92dc;
    public const uint GL_MAX_ATOMIC_COUNTER_BUFFER_SIZE = 0x92d8;
    public const uint GL_MAX_ATTRIB_STACK_DEPTH = 0xd35;
    public const uint GL_MAX_CLIENT_ATTRIB_STACK_DEPTH = 0xd3b;
    public const uint GL_MAX_CLIP_DISTANCES = 0xd32;
    public const uint GL_MAX_CLIP_PLANES = 0xd32;
    public const uint GL_MAX_COLOR_ATTACHMENTS = 0x8cdf;
    public const uint GL_MAX_COLOR_TEXTURE_SAMPLES = 0x910e;
    public const uint GL_MAX_COMBINED_ATOMIC_COUNTERS = 0x92d7;
    public const uint GL_MAX_COMBINED_ATOMIC_COUNTER_BUFFERS = 0x92d1;
    public const uint GL_MAX_COMBINED_CLIP_AND_CULL_DISTANCES = 0x82fa;
    public const uint GL_MAX_COMBINED_COMPUTE_UNIFORM_COMPONENTS = 0x8266;
    public const uint GL_MAX_COMBINED_DIMENSIONS = 0x8282;
    public const uint GL_MAX_COMBINED_FRAGMENT_UNIFORM_COMPONENTS = 0x8a33;
    public const uint GL_MAX_COMBINED_GEOMETRY_UNIFORM_COMPONENTS = 0x8a32;
    public const uint GL_MAX_COMBINED_IMAGE_UNIFORMS = 0x90cf;
    public const uint GL_MAX_COMBINED_IMAGE_UNITS_AND_FRAGMENT_OUTPUTS = 0x8f39;
    public const uint GL_MAX_COMBINED_SHADER_OUTPUT_RESOURCES = 0x8f39;
    public const uint GL_MAX_COMBINED_SHADER_STORAGE_BLOCKS = 0x90dc;
    public const uint GL_MAX_COMBINED_TESS_CONTROL_UNIFORM_COMPONENTS = 0x8e1e;
    public const uint GL_MAX_COMBINED_TESS_EVALUATION_UNIFORM_COMPONENTS = 0x8e1f;
    public const uint GL_MAX_COMBINED_TEXTURE_IMAGE_UNITS = 0x8b4d;
    public const uint GL_MAX_COMBINED_UNIFORM_BLOCKS = 0x8a2e;
    public const uint GL_MAX_COMBINED_VERTEX_UNIFORM_COMPONENTS = 0x8a31;
    public const uint GL_MAX_COMPUTE_ATOMIC_COUNTERS = 0x8265;
    public const uint GL_MAX_COMPUTE_ATOMIC_COUNTER_BUFFERS = 0x8264;
    public const uint GL_MAX_COMPUTE_FIXED_GROUP_INVOCATIONS_ARB = 0x90eb;
    public const uint GL_MAX_COMPUTE_FIXED_GROUP_SIZE_ARB = 0x91bf;
    public const uint GL_MAX_COMPUTE_IMAGE_UNIFORMS = 0x91bd;
    public const uint GL_MAX_COMPUTE_SHADER_STORAGE_BLOCKS = 0x90db;
    public const uint GL_MAX_COMPUTE_SHARED_MEMORY_SIZE = 0x8262;
    public const uint GL_MAX_COMPUTE_TEXTURE_IMAGE_UNITS = 0x91bc;
    public const uint GL_MAX_COMPUTE_UNIFORM_BLOCKS = 0x91bb;
    public const uint GL_MAX_COMPUTE_UNIFORM_COMPONENTS = 0x8263;
    public const uint GL_MAX_COMPUTE_VARIABLE_GROUP_INVOCATIONS_ARB = 0x9344;
    public const uint GL_MAX_COMPUTE_VARIABLE_GROUP_SIZE_ARB = 0x9345;
    public const uint GL_MAX_COMPUTE_WORK_GROUP_COUNT = 0x91be;
    public const uint GL_MAX_COMPUTE_WORK_GROUP_INVOCATIONS = 0x90eb;
    public const uint GL_MAX_COMPUTE_WORK_GROUP_SIZE = 0x91bf;
    public const uint GL_MAX_CUBE_MAP_TEXTURE_SIZE = 0x851c;
    public const uint GL_MAX_CULL_DISTANCES = 0x82f9;
    public const uint GL_MAX_DEBUG_GROUP_STACK_DEPTH = 0x826c;
    public const uint GL_MAX_DEBUG_LOGGED_MESSAGES = 0x9144;
    public const uint GL_MAX_DEBUG_LOGGED_MESSAGES_ARB = 0x9144;
    public const uint GL_MAX_DEBUG_MESSAGE_LENGTH = 0x9143;
    public const uint GL_MAX_DEBUG_MESSAGE_LENGTH_ARB = 0x9143;
    public const uint GL_MAX_DEPTH = 0x8280;
    public const uint GL_MAX_DEPTH_TEXTURE_SAMPLES = 0x910f;
    public const uint GL_MAX_DRAW_BUFFERS = 0x8824;
    public const uint GL_MAX_DUAL_SOURCE_DRAW_BUFFERS = 0x88fc;
    public const uint GL_MAX_ELEMENTS_INDICES = 0x80e9;
    public const uint GL_MAX_ELEMENTS_INDICES_WIN = 0x80e9;
    public const uint GL_MAX_ELEMENTS_VERTICES = 0x80e8;
    public const uint GL_MAX_ELEMENTS_VERTICES_WIN = 0x80e8;
    public const uint GL_MAX_ELEMENT_INDEX = 0x8d6b;
    public const uint GL_MAX_EVAL_ORDER = 0xd30;
    public const uint GL_MAX_FRAGMENT_ATOMIC_COUNTERS = 0x92d6;
    public const uint GL_MAX_FRAGMENT_ATOMIC_COUNTER_BUFFERS = 0x92d0;
    public const uint GL_MAX_FRAGMENT_IMAGE_UNIFORMS = 0x90ce;
    public const uint GL_MAX_FRAGMENT_INPUT_COMPONENTS = 0x9125;
    public const uint GL_MAX_FRAGMENT_INTERPOLATION_OFFSET = 0x8e5c;
    public const uint GL_MAX_FRAGMENT_SHADER_STORAGE_BLOCKS = 0x90da;
    public const uint GL_MAX_FRAGMENT_UNIFORM_BLOCKS = 0x8a2d;
    public const uint GL_MAX_FRAGMENT_UNIFORM_COMPONENTS = 0x8b49;
    public const uint GL_MAX_FRAGMENT_UNIFORM_VECTORS = 0x8dfd;
    public const uint GL_MAX_FRAMEBUFFER_HEIGHT = 0x9316;
    public const uint GL_MAX_FRAMEBUFFER_LAYERS = 0x9317;
    public const uint GL_MAX_FRAMEBUFFER_SAMPLES = 0x9318;
    public const uint GL_MAX_FRAMEBUFFER_WIDTH = 0x9315;
    public const uint GL_MAX_GEOMETRY_ATOMIC_COUNTERS = 0x92d5;
    public const uint GL_MAX_GEOMETRY_ATOMIC_COUNTER_BUFFERS = 0x92cf;
    public const uint GL_MAX_GEOMETRY_IMAGE_UNIFORMS = 0x90cd;
    public const uint GL_MAX_GEOMETRY_INPUT_COMPONENTS = 0x9123;
    public const uint GL_MAX_GEOMETRY_OUTPUT_COMPONENTS = 0x9124;
    public const uint GL_MAX_GEOMETRY_OUTPUT_VERTICES = 0x8de0;
    public const uint GL_MAX_GEOMETRY_SHADER_INVOCATIONS = 0x8e5a;
    public const uint GL_MAX_GEOMETRY_SHADER_STORAGE_BLOCKS = 0x90d7;
    public const uint GL_MAX_GEOMETRY_TEXTURE_IMAGE_UNITS = 0x8c29;
    public const uint GL_MAX_GEOMETRY_TOTAL_OUTPUT_COMPONENTS = 0x8de1;
    public const uint GL_MAX_GEOMETRY_UNIFORM_BLOCKS = 0x8a2c;
    public const uint GL_MAX_GEOMETRY_UNIFORM_COMPONENTS = 0x8ddf;
    public const uint GL_MAX_HEIGHT = 0x827f;
    public const uint GL_MAX_IMAGE_SAMPLES = 0x906d;
    public const uint GL_MAX_IMAGE_UNITS = 0x8f38;
    public const uint GL_MAX_INTEGER_SAMPLES = 0x9110;
    public const uint GL_MAX_LABEL_LENGTH = 0x82e8;
    public const uint GL_MAX_LAYERS = 0x8281;
    public const uint GL_MAX_LIGHTS = 0xd31;
    public const uint GL_MAX_LIST_NESTING = 0xb31;
    public const uint GL_MAX_MODELVIEW_STACK_DEPTH = 0xd36;
    public const uint GL_MAX_NAME_LENGTH = 0x92f6;
    public const uint GL_MAX_NAME_STACK_DEPTH = 0xd37;
    public const uint GL_MAX_NUM_ACTIVE_VARIABLES = 0x92f7;
    public const uint GL_MAX_NUM_COMPATIBLE_SUBROUTINES = 0x92f8;
    public const uint GL_MAX_PATCH_VERTICES = 0x8e7d;
    public const uint GL_MAX_PIXEL_MAP_TABLE = 0xd34;
    public const uint GL_MAX_PROGRAM_TEXEL_OFFSET = 0x8905;
    public const uint GL_MAX_PROGRAM_TEXTURE_GATHER_COMPONENTS_ARB = 0x8f9f;
    public const uint GL_MAX_PROGRAM_TEXTURE_GATHER_OFFSET = 0x8e5f;
    public const uint GL_MAX_PROGRAM_TEXTURE_GATHER_OFFSET_ARB = 0x8e5f;
    public const uint GL_MAX_PROJECTION_STACK_DEPTH = 0xd38;
    public const uint GL_MAX_RECTANGLE_TEXTURE_SIZE = 0x84f8;
    public const uint GL_MAX_RENDERBUFFER_SIZE = 0x84e8;
    public const uint GL_MAX_SAMPLES = 0x8d57;
    public const uint GL_MAX_SAMPLE_MASK_WORDS = 0x8e59;
    public const uint GL_MAX_SERVER_WAIT_TIMEOUT = 0x9111;
    public const uint GL_MAX_SHADER_STORAGE_BLOCK_SIZE = 0x90de;
    public const uint GL_MAX_SHADER_STORAGE_BUFFER_BINDINGS = 0x90dd;
    public const uint GL_MAX_SPARSE_3D_TEXTURE_SIZE_ARB = 0x9199;
    public const uint GL_MAX_SPARSE_ARRAY_TEXTURE_LAYERS_ARB = 0x919a;
    public const uint GL_MAX_SPARSE_TEXTURE_SIZE_ARB = 0x9198;
    public const uint GL_MAX_SUBROUTINES = 0x8de7;
    public const uint GL_MAX_SUBROUTINE_UNIFORM_LOCATIONS = 0x8de8;
    public const uint GL_MAX_TESS_CONTROL_ATOMIC_COUNTERS = 0x92d3;
    public const uint GL_MAX_TESS_CONTROL_ATOMIC_COUNTER_BUFFERS = 0x92cd;
    public const uint GL_MAX_TESS_CONTROL_IMAGE_UNIFORMS = 0x90cb;
    public const uint GL_MAX_TESS_CONTROL_INPUT_COMPONENTS = 0x886c;
    public const uint GL_MAX_TESS_CONTROL_OUTPUT_COMPONENTS = 0x8e83;
    public const uint GL_MAX_TESS_CONTROL_SHADER_STORAGE_BLOCKS = 0x90d8;
    public const uint GL_MAX_TESS_CONTROL_TEXTURE_IMAGE_UNITS = 0x8e81;
    public const uint GL_MAX_TESS_CONTROL_TOTAL_OUTPUT_COMPONENTS = 0x8e85;
    public const uint GL_MAX_TESS_CONTROL_UNIFORM_BLOCKS = 0x8e89;
    public const uint GL_MAX_TESS_CONTROL_UNIFORM_COMPONENTS = 0x8e7f;
    public const uint GL_MAX_TESS_EVALUATION_ATOMIC_COUNTERS = 0x92d4;
    public const uint GL_MAX_TESS_EVALUATION_ATOMIC_COUNTER_BUFFERS = 0x92ce;
    public const uint GL_MAX_TESS_EVALUATION_IMAGE_UNIFORMS = 0x90cc;
    public const uint GL_MAX_TESS_EVALUATION_INPUT_COMPONENTS = 0x886d;
    public const uint GL_MAX_TESS_EVALUATION_OUTPUT_COMPONENTS = 0x8e86;
    public const uint GL_MAX_TESS_EVALUATION_SHADER_STORAGE_BLOCKS = 0x90d9;
    public const uint GL_MAX_TESS_EVALUATION_TEXTURE_IMAGE_UNITS = 0x8e82;
    public const uint GL_MAX_TESS_EVALUATION_UNIFORM_BLOCKS = 0x8e8a;
    public const uint GL_MAX_TESS_EVALUATION_UNIFORM_COMPONENTS = 0x8e80;
    public const uint GL_MAX_TESS_GEN_LEVEL = 0x8e7e;
    public const uint GL_MAX_TESS_PATCH_COMPONENTS = 0x8e84;
    public const uint GL_MAX_TEXTURE_BUFFER_SIZE = 0x8c2b;
    public const uint GL_MAX_TEXTURE_IMAGE_UNITS = 0x8872;
    public const uint GL_MAX_TEXTURE_LOD_BIAS = 0x84fd;
    public const uint GL_MAX_TEXTURE_SIZE = 0xd33;
    public const uint GL_MAX_TEXTURE_STACK_DEPTH = 0xd39;
    public const uint GL_MAX_TRANSFORM_FEEDBACK_BUFFERS = 0x8e70;
    public const uint GL_MAX_TRANSFORM_FEEDBACK_INTERLEAVED_COMPONENTS = 0x8c8a;
    public const uint GL_MAX_TRANSFORM_FEEDBACK_SEPARATE_ATTRIBS = 0x8c8b;
    public const uint GL_MAX_TRANSFORM_FEEDBACK_SEPARATE_COMPONENTS = 0x8c80;
    public const uint GL_MAX_UNIFORM_BLOCK_SIZE = 0x8a30;
    public const uint GL_MAX_UNIFORM_BUFFER_BINDINGS = 0x8a2f;
    public const uint GL_MAX_UNIFORM_LOCATIONS = 0x826e;
    public const uint GL_MAX_VARYING_COMPONENTS = 0x8b4b;
    public const uint GL_MAX_VARYING_FLOATS = 0x8b4b;
    public const uint GL_MAX_VARYING_VECTORS = 0x8dfc;
    public const uint GL_MAX_VERTEX_ATOMIC_COUNTERS = 0x92d2;
    public const uint GL_MAX_VERTEX_ATOMIC_COUNTER_BUFFERS = 0x92cc;
    public const uint GL_MAX_VERTEX_ATTRIBS = 0x8869;
    public const uint GL_MAX_VERTEX_ATTRIB_BINDINGS = 0x82da;
    public const uint GL_MAX_VERTEX_ATTRIB_RELATIVE_OFFSET = 0x82d9;
    public const uint GL_MAX_VERTEX_ATTRIB_STRIDE = 0x82e5;
    public const uint GL_MAX_VERTEX_IMAGE_UNIFORMS = 0x90ca;
    public const uint GL_MAX_VERTEX_OUTPUT_COMPONENTS = 0x9122;
    public const uint GL_MAX_VERTEX_SHADER_STORAGE_BLOCKS = 0x90d6;
    public const uint GL_MAX_VERTEX_STREAMS = 0x8e71;
    public const uint GL_MAX_VERTEX_TEXTURE_IMAGE_UNITS = 0x8b4c;
    public const uint GL_MAX_VERTEX_UNIFORM_BLOCKS = 0x8a2b;
    public const uint GL_MAX_VERTEX_UNIFORM_COMPONENTS = 0x8b4a;
    public const uint GL_MAX_VERTEX_UNIFORM_VECTORS = 0x8dfb;
    public const uint GL_MAX_VIEWPORTS = 0x825b;
    public const uint GL_MAX_VIEWPORT_DIMS = 0xd3a;
    public const uint GL_MAX_WIDTH = 0x827e;
    public const uint GL_MEDIUM_FLOAT = 0x8df1;
    public const uint GL_MEDIUM_INT = 0x8df4;
    public const uint GL_MIN = 0x8007;
    public const uint GL_MINOR_VERSION = 0x821c;
    public const uint GL_MIN_FRAGMENT_INTERPOLATION_OFFSET = 0x8e5b;
    public const uint GL_MIN_MAP_BUFFER_ALIGNMENT = 0x90bc;
    public const uint GL_MIN_PROGRAM_TEXEL_OFFSET = 0x8904;
    public const uint GL_MIN_PROGRAM_TEXTURE_GATHER_OFFSET = 0x8e5e;
    public const uint GL_MIN_PROGRAM_TEXTURE_GATHER_OFFSET_ARB = 0x8e5e;
    public const uint GL_MIN_SAMPLE_SHADING_VALUE = 0x8c37;
    public const uint GL_MIN_SAMPLE_SHADING_VALUE_ARB = 0x8c37;
    public const uint GL_MIPMAP = 0x8293;
    public const uint GL_MIRRORED_REPEAT = 0x8370;
    public const uint GL_MIRROR_CLAMP_TO_EDGE = 0x8743;
    public const uint GL_MODELVIEW = 0x1700;
    public const uint GL_MODELVIEW_MATRIX = 0xba6;
    public const uint GL_MODELVIEW_STACK_DEPTH = 0xba3;
    public const uint GL_MODULATE = 0x2100;
    public const uint GL_MULT = 0x103;
    public const uint GL_MULTISAMPLE = 0x809d;
    public const uint GL_N3F_V3F = 0x2a25;
    public const uint GL_NAMED_STRING_LENGTH_ARB = 0x8de9;
    public const uint GL_NAMED_STRING_TYPE_ARB = 0x8dea;
    public const uint GL_NAME_LENGTH = 0x92f9;
    public const uint GL_NAME_STACK_DEPTH = 0xd70;
    public const uint GL_NAND = 0x150e;
    public const uint GL_NEAREST = 0x2600;
    public const uint GL_NEAREST_MIPMAP_LINEAR = 0x2702;
    public const uint GL_NEAREST_MIPMAP_NEAREST = 0x2700;
    public const uint GL_NEGATIVE_ONE_TO_ONE = 0x935e;
    public const uint GL_NEVER = 0x200;
    public const uint GL_NICEST = 0x1102;
    public const uint GL_NONE = 0x0;
    public const uint GL_NOOP = 0x1505;
    public const uint GL_NOR = 0x1508;
    public const uint GL_NORMALIZE = 0xba1;
    public const uint GL_NORMAL_ARRAY = 0x8075;
    public const uint GL_NORMAL_ARRAY_COUNT_EXT = 0x8080;
    public const uint GL_NORMAL_ARRAY_EXT = 0x8075;
    public const uint GL_NORMAL_ARRAY_POINTER = 0x808f;
    public const uint GL_NORMAL_ARRAY_POINTER_EXT = 0x808f;
    public const uint GL_NORMAL_ARRAY_STRIDE = 0x807f;
    public const uint GL_NORMAL_ARRAY_STRIDE_EXT = 0x807f;
    public const uint GL_NORMAL_ARRAY_TYPE = 0x807e;
    public const uint GL_NORMAL_ARRAY_TYPE_EXT = 0x807e;
    public const uint GL_NOTEQUAL = 0x205;
    public const uint GL_NO_ERROR = 0x0;
    public const uint GL_NO_RESET_NOTIFICATION = 0x8261;
    public const uint GL_NO_RESET_NOTIFICATION_ARB = 0x8261;
    public const uint GL_NUM_ACTIVE_VARIABLES = 0x9304;
    public const uint GL_NUM_COMPATIBLE_SUBROUTINES = 0x8e4a;
    public const uint GL_NUM_COMPRESSED_TEXTURE_FORMATS = 0x86a2;
    public const uint GL_NUM_EXTENSIONS = 0x821d;
    public const uint GL_NUM_PROGRAM_BINARY_FORMATS = 0x87fe;
    public const uint GL_NUM_SAMPLE_COUNTS = 0x9380;
    public const uint GL_NUM_SHADER_BINARY_FORMATS = 0x8df9;
    public const uint GL_NUM_SHADING_LANGUAGE_VERSIONS = 0x82e9;
    public const uint GL_NUM_SPARSE_LEVELS_ARB = 0x91aa;
    public const uint GL_NUM_VIRTUAL_PAGE_SIZES_ARB = 0x91a8;
    public const uint GL_OBJECT_LINEAR = 0x2401;
    public const uint GL_OBJECT_PLANE = 0x2501;
    public const uint GL_OBJECT_TYPE = 0x9112;
    public const uint GL_OFFSET = 0x92fc;
    public const uint GL_ONE = 0x1;
    public const uint GL_ONE_MINUS_CONSTANT_ALPHA = 0x8004;
    public const uint GL_ONE_MINUS_CONSTANT_COLOR = 0x8002;
    public const uint GL_ONE_MINUS_DST_ALPHA = 0x305;
    public const uint GL_ONE_MINUS_DST_COLOR = 0x307;
    public const uint GL_ONE_MINUS_SRC1_ALPHA = 0x88fb;
    public const uint GL_ONE_MINUS_SRC1_COLOR = 0x88fa;
    public const uint GL_ONE_MINUS_SRC_ALPHA = 0x303;
    public const uint GL_ONE_MINUS_SRC_COLOR = 0x301;
    public const uint GL_OR = 0x1507;
    public const uint GL_ORDER = 0xa01;
    public const uint GL_OR_INVERTED = 0x150d;
    public const uint GL_OR_REVERSE = 0x150b;
    public const uint GL_OUT_OF_MEMORY = 0x505;
    public const uint GL_PACK_ALIGNMENT = 0xd05;
    public const uint GL_PACK_COMPRESSED_BLOCK_DEPTH = 0x912d;
    public const uint GL_PACK_COMPRESSED_BLOCK_HEIGHT = 0x912c;
    public const uint GL_PACK_COMPRESSED_BLOCK_SIZE = 0x912e;
    public const uint GL_PACK_COMPRESSED_BLOCK_WIDTH = 0x912b;
    public const uint GL_PACK_IMAGE_HEIGHT = 0x806c;
    public const uint GL_PACK_LSB_FIRST = 0xd01;
    public const uint GL_PACK_ROW_LENGTH = 0xd02;
    public const uint GL_PACK_SKIP_IMAGES = 0x806b;
    public const uint GL_PACK_SKIP_PIXELS = 0xd04;
    public const uint GL_PACK_SKIP_ROWS = 0xd03;
    public const uint GL_PACK_SWAP_BYTES = 0xd00;
    public const uint GL_PARAMETER_BUFFER_ARB = 0x80ee;
    public const uint GL_PARAMETER_BUFFER_BINDING_ARB = 0x80ef;
    public const uint GL_PASS_THROUGH_TOKEN = 0x700;
    public const uint GL_PATCHES = 0xe;
    public const uint GL_PATCH_DEFAULT_INNER_LEVEL = 0x8e73;
    public const uint GL_PATCH_DEFAULT_OUTER_LEVEL = 0x8e74;
    public const uint GL_PATCH_VERTICES = 0x8e72;
    public const uint GL_PERSPECTIVE_CORRECTION_HINT = 0xc50;
    public const uint GL_PHONG_HINT_WIN = 0x80eb;
    public const uint GL_PHONG_WIN = 0x80ea;
    public const uint GL_PIXEL_BUFFER_BARRIER_BIT = 0x80;
    public const uint GL_PIXEL_MAP_A_TO_A = 0xc79;
    public const uint GL_PIXEL_MAP_A_TO_A_SIZE = 0xcb9;
    public const uint GL_PIXEL_MAP_B_TO_B = 0xc78;
    public const uint GL_PIXEL_MAP_B_TO_B_SIZE = 0xcb8;
    public const uint GL_PIXEL_MAP_G_TO_G = 0xc77;
    public const uint GL_PIXEL_MAP_G_TO_G_SIZE = 0xcb7;
    public const uint GL_PIXEL_MAP_I_TO_A = 0xc75;
    public const uint GL_PIXEL_MAP_I_TO_A_SIZE = 0xcb5;
    public const uint GL_PIXEL_MAP_I_TO_B = 0xc74;
    public const uint GL_PIXEL_MAP_I_TO_B_SIZE = 0xcb4;
    public const uint GL_PIXEL_MAP_I_TO_G = 0xc73;
    public const uint GL_PIXEL_MAP_I_TO_G_SIZE = 0xcb3;
    public const uint GL_PIXEL_MAP_I_TO_I = 0xc70;
    public const uint GL_PIXEL_MAP_I_TO_I_SIZE = 0xcb0;
    public const uint GL_PIXEL_MAP_I_TO_R = 0xc72;
    public const uint GL_PIXEL_MAP_I_TO_R_SIZE = 0xcb2;
    public const uint GL_PIXEL_MAP_R_TO_R = 0xc76;
    public const uint GL_PIXEL_MAP_R_TO_R_SIZE = 0xcb6;
    public const uint GL_PIXEL_MAP_S_TO_S = 0xc71;
    public const uint GL_PIXEL_MAP_S_TO_S_SIZE = 0xcb1;
    public const uint GL_PIXEL_MODE_BIT = 0x20;
    public const uint GL_PIXEL_PACK_BUFFER = 0x88eb;
    public const uint GL_PIXEL_PACK_BUFFER_BINDING = 0x88ed;
    public const uint GL_PIXEL_UNPACK_BUFFER = 0x88ec;
    public const uint GL_PIXEL_UNPACK_BUFFER_BINDING = 0x88ef;
    public const uint GL_POINT = 0x1b00;
    public const uint GL_POINTS = 0x0;
    public const uint GL_POINT_BIT = 0x2;
    public const uint GL_POINT_FADE_THRESHOLD_SIZE = 0x8128;
    public const uint GL_POINT_SIZE = 0xb11;
    public const uint GL_POINT_SIZE_GRANULARITY = 0xb13;
    public const uint GL_POINT_SIZE_RANGE = 0xb12;
    public const uint GL_POINT_SMOOTH = 0xb10;
    public const uint GL_POINT_SMOOTH_HINT = 0xc51;
    public const uint GL_POINT_SPRITE_COORD_ORIGIN = 0x8ca0;
    public const uint GL_POINT_TOKEN = 0x701;
    public const uint GL_POLYGON = 0x9;
    public const uint GL_POLYGON_BIT = 0x8;
    public const uint GL_POLYGON_MODE = 0xb40;
    public const uint GL_POLYGON_OFFSET_FACTOR = 0x8038;
    public const uint GL_POLYGON_OFFSET_FILL = 0x8037;
    public const uint GL_POLYGON_OFFSET_LINE = 0x2a02;
    public const uint GL_POLYGON_OFFSET_POINT = 0x2a01;
    public const uint GL_POLYGON_OFFSET_UNITS = 0x2a00;
    public const uint GL_POLYGON_SMOOTH = 0xb41;
    public const uint GL_POLYGON_SMOOTH_HINT = 0xc53;
    public const uint GL_POLYGON_STIPPLE = 0xb42;
    public const uint GL_POLYGON_STIPPLE_BIT = 0x10;
    public const uint GL_POLYGON_TOKEN = 0x703;
    public const uint GL_POSITION = 0x1203;
    public const uint GL_PRIMITIVES_GENERATED = 0x8c87;
    public const uint GL_PRIMITIVES_SUBMITTED_ARB = 0x82ef;
    public const uint GL_PRIMITIVE_RESTART = 0x8f9d;
    public const uint GL_PRIMITIVE_RESTART_FIXED_INDEX = 0x8d69;
    public const uint GL_PRIMITIVE_RESTART_FOR_PATCHES_SUPPORTED = 0x8221;
    public const uint GL_PRIMITIVE_RESTART_INDEX = 0x8f9e;
    public const uint GL_PROGRAM = 0x82e2;
    public const uint GL_PROGRAM_BINARY_FORMATS = 0x87ff;
    public const uint GL_PROGRAM_BINARY_LENGTH = 0x8741;
    public const uint GL_PROGRAM_BINARY_RETRIEVABLE_HINT = 0x8257;
    public const uint GL_PROGRAM_INPUT = 0x92e3;
    public const uint GL_PROGRAM_OUTPUT = 0x92e4;
    public const uint GL_PROGRAM_PIPELINE = 0x82e4;
    public const uint GL_PROGRAM_PIPELINE_BINDING = 0x825a;
    public const uint GL_PROGRAM_POINT_SIZE = 0x8642;
    public const uint GL_PROGRAM_SEPARABLE = 0x8258;
    public const uint GL_PROJECTION = 0x1701;
    public const uint GL_PROJECTION_MATRIX = 0xba7;
    public const uint GL_PROJECTION_STACK_DEPTH = 0xba4;
    public const uint GL_PROVOKING_VERTEX = 0x8e4f;
    public const uint GL_PROXY_TEXTURE_1D = 0x8063;
    public const uint GL_PROXY_TEXTURE_1D_ARRAY = 0x8c19;
    public const uint GL_PROXY_TEXTURE_2D = 0x8064;
    public const uint GL_PROXY_TEXTURE_2D_ARRAY = 0x8c1b;
    public const uint GL_PROXY_TEXTURE_2D_MULTISAMPLE = 0x9101;
    public const uint GL_PROXY_TEXTURE_2D_MULTISAMPLE_ARRAY = 0x9103;
    public const uint GL_PROXY_TEXTURE_3D = 0x8070;
    public const uint GL_PROXY_TEXTURE_CUBE_MAP = 0x851b;
    public const uint GL_PROXY_TEXTURE_CUBE_MAP_ARRAY = 0x900b;
    public const uint GL_PROXY_TEXTURE_CUBE_MAP_ARRAY_ARB = 0x900b;
    public const uint GL_PROXY_TEXTURE_RECTANGLE = 0x84f7;
    public const uint GL_Q = 0x2003;
    public const uint GL_QUADRATIC_ATTENUATION = 0x1209;
    public const uint GL_QUADS = 0x7;
    public const uint GL_QUADS_FOLLOW_PROVOKING_VERTEX_CONVENTION = 0x8e4c;
    public const uint GL_QUAD_STRIP = 0x8;
    public const uint GL_QUERY = 0x82e3;
    public const uint GL_QUERY_BUFFER = 0x9192;
    public const uint GL_QUERY_BUFFER_BARRIER_BIT = 0x8000;
    public const uint GL_QUERY_BUFFER_BINDING = 0x9193;
    public const uint GL_QUERY_BY_REGION_NO_WAIT = 0x8e16;
    public const uint GL_QUERY_BY_REGION_NO_WAIT_INVERTED = 0x8e1a;
    public const uint GL_QUERY_BY_REGION_WAIT = 0x8e15;
    public const uint GL_QUERY_BY_REGION_WAIT_INVERTED = 0x8e19;
    public const uint GL_QUERY_COUNTER_BITS = 0x8864;
    public const uint GL_QUERY_NO_WAIT = 0x8e14;
    public const uint GL_QUERY_NO_WAIT_INVERTED = 0x8e18;
    public const uint GL_QUERY_RESULT = 0x8866;
    public const uint GL_QUERY_RESULT_AVAILABLE = 0x8867;
    public const uint GL_QUERY_RESULT_NO_WAIT = 0x9194;
    public const uint GL_QUERY_TARGET = 0x82ea;
    public const uint GL_QUERY_WAIT = 0x8e13;
    public const uint GL_QUERY_WAIT_INVERTED = 0x8e17;
    public const uint GL_R = 0x2002;
    public const uint GL_R11F_G11F_B10F = 0x8c3a;
    public const uint GL_R16 = 0x822a;
    public const uint GL_R16F = 0x822d;
    public const uint GL_R16I = 0x8233;
    public const uint GL_R16UI = 0x8234;
    public const uint GL_R16_SNORM = 0x8f98;
    public const uint GL_R32F = 0x822e;
    public const uint GL_R32I = 0x8235;
    public const uint GL_R32UI = 0x8236;
    public const uint GL_R3_G3_B2 = 0x2a10;
    public const uint GL_R8 = 0x8229;
    public const uint GL_R8I = 0x8231;
    public const uint GL_R8UI = 0x8232;
    public const uint GL_R8_SNORM = 0x8f94;
    public const uint GL_RASTERIZER_DISCARD = 0x8c89;
    public const uint GL_READ_BUFFER = 0xc02;
    public const uint GL_READ_FRAMEBUFFER = 0x8ca8;
    public const uint GL_READ_FRAMEBUFFER_BINDING = 0x8caa;
    public const uint GL_READ_ONLY = 0x88b8;
    public const uint GL_READ_PIXELS = 0x828c;
    public const uint GL_READ_PIXELS_FORMAT = 0x828d;
    public const uint GL_READ_PIXELS_TYPE = 0x828e;
    public const uint GL_READ_WRITE = 0x88ba;
    public const uint GL_RED = 0x1903;
    public const uint GL_RED_BIAS = 0xd15;
    public const uint GL_RED_BITS = 0xd52;
    public const uint GL_RED_INTEGER = 0x8d94;
    public const uint GL_RED_SCALE = 0xd14;
    public const uint GL_REFERENCED_BY_COMPUTE_SHADER = 0x930b;
    public const uint GL_REFERENCED_BY_FRAGMENT_SHADER = 0x930a;
    public const uint GL_REFERENCED_BY_GEOMETRY_SHADER = 0x9309;
    public const uint GL_REFERENCED_BY_TESS_CONTROL_SHADER = 0x9307;
    public const uint GL_REFERENCED_BY_TESS_EVALUATION_SHADER = 0x9308;
    public const uint GL_REFERENCED_BY_VERTEX_SHADER = 0x9306;
    public const uint GL_RENDER = 0x1c00;
    public const uint GL_RENDERBUFFER = 0x8d41;
    public const uint GL_RENDERBUFFER_ALPHA_SIZE = 0x8d53;
    public const uint GL_RENDERBUFFER_BINDING = 0x8ca7;
    public const uint GL_RENDERBUFFER_BLUE_SIZE = 0x8d52;
    public const uint GL_RENDERBUFFER_DEPTH_SIZE = 0x8d54;
    public const uint GL_RENDERBUFFER_GREEN_SIZE = 0x8d51;
    public const uint GL_RENDERBUFFER_HEIGHT = 0x8d43;
    public const uint GL_RENDERBUFFER_INTERNAL_FORMAT = 0x8d44;
    public const uint GL_RENDERBUFFER_RED_SIZE = 0x8d50;
    public const uint GL_RENDERBUFFER_SAMPLES = 0x8cab;
    public const uint GL_RENDERBUFFER_STENCIL_SIZE = 0x8d55;
    public const uint GL_RENDERBUFFER_WIDTH = 0x8d42;
    public const uint GL_RENDERER = 0x1f01;
    public const uint GL_RENDER_MODE = 0xc40;
    public const uint GL_REPEAT = 0x2901;
    public const uint GL_REPLACE = 0x1e01;
    public const uint GL_RESET_NOTIFICATION_STRATEGY = 0x8256;
    public const uint GL_RESET_NOTIFICATION_STRATEGY_ARB = 0x8256;
    public const uint GL_RETURN = 0x102;
    public const uint GL_RG = 0x8227;
    public const uint GL_RG16 = 0x822c;
    public const uint GL_RG16F = 0x822f;
    public const uint GL_RG16I = 0x8239;
    public const uint GL_RG16UI = 0x823a;
    public const uint GL_RG16_SNORM = 0x8f99;
    public const uint GL_RG32F = 0x8230;
    public const uint GL_RG32I = 0x823b;
    public const uint GL_RG32UI = 0x823c;
    public const uint GL_RG8 = 0x822b;
    public const uint GL_RG8I = 0x8237;
    public const uint GL_RG8UI = 0x8238;
    public const uint GL_RG8_SNORM = 0x8f95;
    public const uint GL_RGB = 0x1907;
    public const uint GL_RGB10 = 0x8052;
    public const uint GL_RGB10_A2 = 0x8059;
    public const uint GL_RGB10_A2UI = 0x906f;
    public const uint GL_RGB12 = 0x8053;
    public const uint GL_RGB16 = 0x8054;
    public const uint GL_RGB16F = 0x881b;
    public const uint GL_RGB16I = 0x8d89;
    public const uint GL_RGB16UI = 0x8d77;
    public const uint GL_RGB16_SNORM = 0x8f9a;
    public const uint GL_RGB32F = 0x8815;
    public const uint GL_RGB32I = 0x8d83;
    public const uint GL_RGB32UI = 0x8d71;
    public const uint GL_RGB4 = 0x804f;
    public const uint GL_RGB5 = 0x8050;
    public const uint GL_RGB565 = 0x8d62;
    public const uint GL_RGB5_A1 = 0x8057;
    public const uint GL_RGB8 = 0x8051;
    public const uint GL_RGB8I = 0x8d8f;
    public const uint GL_RGB8UI = 0x8d7d;
    public const uint GL_RGB8_SNORM = 0x8f96;
    public const uint GL_RGB9_E5 = 0x8c3d;
    public const uint GL_RGBA = 0x1908;
    public const uint GL_RGBA12 = 0x805a;
    public const uint GL_RGBA16 = 0x805b;
    public const uint GL_RGBA16F = 0x881a;
    public const uint GL_RGBA16I = 0x8d88;
    public const uint GL_RGBA16UI = 0x8d76;
    public const uint GL_RGBA16_SNORM = 0x8f9b;
    public const uint GL_RGBA2 = 0x8055;
    public const uint GL_RGBA32F = 0x8814;
    public const uint GL_RGBA32I = 0x8d82;
    public const uint GL_RGBA32UI = 0x8d70;
    public const uint GL_RGBA4 = 0x8056;
    public const uint GL_RGBA8 = 0x8058;
    public const uint GL_RGBA8I = 0x8d8e;
    public const uint GL_RGBA8UI = 0x8d7c;
    public const uint GL_RGBA8_SNORM = 0x8f97;
    public const uint GL_RGBA_INTEGER = 0x8d99;
    public const uint GL_RGBA_MODE = 0xc31;
    public const uint GL_RGB_INTEGER = 0x8d98;
    public const uint GL_RG_INTEGER = 0x8228;
    public const uint GL_RIGHT = 0x407;
    public const uint GL_S = 0x2000;
    public const uint GL_SAMPLER = 0x82e6;
    public const uint GL_SAMPLER_1D = 0x8b5d;
    public const uint GL_SAMPLER_1D_ARRAY = 0x8dc0;
    public const uint GL_SAMPLER_1D_ARRAY_SHADOW = 0x8dc3;
    public const uint GL_SAMPLER_1D_SHADOW = 0x8b61;
    public const uint GL_SAMPLER_2D = 0x8b5e;
    public const uint GL_SAMPLER_2D_ARRAY = 0x8dc1;
    public const uint GL_SAMPLER_2D_ARRAY_SHADOW = 0x8dc4;
    public const uint GL_SAMPLER_2D_MULTISAMPLE = 0x9108;
    public const uint GL_SAMPLER_2D_MULTISAMPLE_ARRAY = 0x910b;
    public const uint GL_SAMPLER_2D_RECT = 0x8b63;
    public const uint GL_SAMPLER_2D_RECT_SHADOW = 0x8b64;
    public const uint GL_SAMPLER_2D_SHADOW = 0x8b62;
    public const uint GL_SAMPLER_3D = 0x8b5f;
    public const uint GL_SAMPLER_BINDING = 0x8919;
    public const uint GL_SAMPLER_BUFFER = 0x8dc2;
    public const uint GL_SAMPLER_CUBE = 0x8b60;
    public const uint GL_SAMPLER_CUBE_MAP_ARRAY = 0x900c;
    public const uint GL_SAMPLER_CUBE_MAP_ARRAY_ARB = 0x900c;
    public const uint GL_SAMPLER_CUBE_MAP_ARRAY_SHADOW = 0x900d;
    public const uint GL_SAMPLER_CUBE_MAP_ARRAY_SHADOW_ARB = 0x900d;
    public const uint GL_SAMPLER_CUBE_SHADOW = 0x8dc5;
    public const uint GL_SAMPLES = 0x80a9;
    public const uint GL_SAMPLES_PASSED = 0x8914;
    public const uint GL_SAMPLE_ALPHA_TO_COVERAGE = 0x809e;
    public const uint GL_SAMPLE_ALPHA_TO_ONE = 0x809f;
    public const uint GL_SAMPLE_BUFFERS = 0x80a8;
    public const uint GL_SAMPLE_COVERAGE = 0x80a0;
    public const uint GL_SAMPLE_COVERAGE_INVERT = 0x80ab;
    public const uint GL_SAMPLE_COVERAGE_VALUE = 0x80aa;
    public const uint GL_SAMPLE_MASK = 0x8e51;
    public const uint GL_SAMPLE_MASK_VALUE = 0x8e52;
    public const uint GL_SAMPLE_POSITION = 0x8e50;
    public const uint GL_SAMPLE_SHADING = 0x8c36;
    public const uint GL_SAMPLE_SHADING_ARB = 0x8c36;
    public const uint GL_SCISSOR_BIT = 0x80000;
    public const uint GL_SCISSOR_BOX = 0xc10;
    public const uint GL_SCISSOR_TEST = 0xc11;
    public const uint GL_SELECT = 0x1c02;
    public const uint GL_SELECTION_BUFFER_POINTER = 0xdf3;
    public const uint GL_SELECTION_BUFFER_SIZE = 0xdf4;
    public const uint GL_SEPARATE_ATTRIBS = 0x8c8d;
    public const uint GL_SET = 0x150f;
    public const uint GL_SHADER = 0x82e1;
    public const uint GL_SHADER_BINARY_FORMATS = 0x8df8;
    public const uint GL_SHADER_COMPILER = 0x8dfa;
    public const uint GL_SHADER_IMAGE_ACCESS_BARRIER_BIT = 0x20;
    public const uint GL_SHADER_IMAGE_ATOMIC = 0x82a6;
    public const uint GL_SHADER_IMAGE_LOAD = 0x82a4;
    public const uint GL_SHADER_IMAGE_STORE = 0x82a5;
    public const uint GL_SHADER_INCLUDE_ARB = 0x8dae;
    public const uint GL_SHADER_SOURCE_LENGTH = 0x8b88;
    public const uint GL_SHADER_STORAGE_BARRIER_BIT = 0x2000;
    public const uint GL_SHADER_STORAGE_BLOCK = 0x92e6;
    public const uint GL_SHADER_STORAGE_BUFFER = 0x90d2;
    public const uint GL_SHADER_STORAGE_BUFFER_BINDING = 0x90d3;
    public const uint GL_SHADER_STORAGE_BUFFER_OFFSET_ALIGNMENT = 0x90df;
    public const uint GL_SHADER_STORAGE_BUFFER_SIZE = 0x90d5;
    public const uint GL_SHADER_STORAGE_BUFFER_START = 0x90d4;
    public const uint GL_SHADER_TYPE = 0x8b4f;
    public const uint GL_SHADE_MODEL = 0xb54;
    public const uint GL_SHADING_LANGUAGE_VERSION = 0x8b8c;
    public const uint GL_SHININESS = 0x1601;
    public const uint GL_SHORT = 0x1402;
    public const uint GL_SIGNALED = 0x9119;
    public const uint GL_SIGNED_NORMALIZED = 0x8f9c;
    public const uint GL_SIMULTANEOUS_TEXTURE_AND_DEPTH_TEST = 0x82ac;
    public const uint GL_SIMULTANEOUS_TEXTURE_AND_DEPTH_WRITE = 0x82ae;
    public const uint GL_SIMULTANEOUS_TEXTURE_AND_STENCIL_TEST = 0x82ad;
    public const uint GL_SIMULTANEOUS_TEXTURE_AND_STENCIL_WRITE = 0x82af;
    public const uint GL_SMOOTH = 0x1d01;
    public const uint GL_SMOOTH_LINE_WIDTH_GRANULARITY = 0xb23;
    public const uint GL_SMOOTH_LINE_WIDTH_RANGE = 0xb22;
    public const uint GL_SMOOTH_POINT_SIZE_GRANULARITY = 0xb13;
    public const uint GL_SMOOTH_POINT_SIZE_RANGE = 0xb12;
    public const uint GL_SPARSE_BUFFER_PAGE_SIZE_ARB = 0x82f8;
    public const uint GL_SPARSE_STORAGE_BIT_ARB = 0x400;
    public const uint GL_SPARSE_TEXTURE_FULL_ARRAY_CUBE_MIPMAPS_ARB = 0x91a9;
    public const uint GL_SPECULAR = 0x1202;
    public const uint GL_SPHERE_MAP = 0x2402;
    public const uint GL_SPOT_CUTOFF = 0x1206;
    public const uint GL_SPOT_DIRECTION = 0x1204;
    public const uint GL_SPOT_EXPONENT = 0x1205;
    public const uint GL_SRC1_ALPHA = 0x8589;
    public const uint GL_SRC1_COLOR = 0x88f9;
    public const uint GL_SRC_ALPHA = 0x302;
    public const uint GL_SRC_ALPHA_SATURATE = 0x308;
    public const uint GL_SRC_COLOR = 0x300;
    public const uint GL_SRGB = 0x8c40;
    public const uint GL_SRGB8 = 0x8c41;
    public const uint GL_SRGB8_ALPHA8 = 0x8c43;
    public const uint GL_SRGB_ALPHA = 0x8c42;
    public const uint GL_SRGB_DECODE_ARB = 0x8299;
    public const uint GL_SRGB_READ = 0x8297;
    public const uint GL_SRGB_WRITE = 0x8298;
    public const uint GL_STACK_OVERFLOW = 0x503;
    public const uint GL_STACK_UNDERFLOW = 0x504;
    public const uint GL_STATIC_COPY = 0x88e6;
    public const uint GL_STATIC_DRAW = 0x88e4;
    public const uint GL_STATIC_READ = 0x88e5;
    public const uint GL_STENCIL = 0x1802;
    public const uint GL_STENCIL_ATTACHMENT = 0x8d20;
    public const uint GL_STENCIL_BACK_FAIL = 0x8801;
    public const uint GL_STENCIL_BACK_FUNC = 0x8800;
    public const uint GL_STENCIL_BACK_PASS_DEPTH_FAIL = 0x8802;
    public const uint GL_STENCIL_BACK_PASS_DEPTH_PASS = 0x8803;
    public const uint GL_STENCIL_BACK_REF = 0x8ca3;
    public const uint GL_STENCIL_BACK_VALUE_MASK = 0x8ca4;
    public const uint GL_STENCIL_BACK_WRITEMASK = 0x8ca5;
    public const uint GL_STENCIL_BITS = 0xd57;
    public const uint GL_STENCIL_BUFFER_BIT = 0x400;
    public const uint GL_STENCIL_CLEAR_VALUE = 0xb91;
    public const uint GL_STENCIL_COMPONENTS = 0x8285;
    public const uint GL_STENCIL_FAIL = 0xb94;
    public const uint GL_STENCIL_FUNC = 0xb92;
    public const uint GL_STENCIL_INDEX = 0x1901;
    public const uint GL_STENCIL_INDEX1 = 0x8d46;
    public const uint GL_STENCIL_INDEX16 = 0x8d49;
    public const uint GL_STENCIL_INDEX4 = 0x8d47;
    public const uint GL_STENCIL_INDEX8 = 0x8d48;
    public const uint GL_STENCIL_PASS_DEPTH_FAIL = 0xb95;
    public const uint GL_STENCIL_PASS_DEPTH_PASS = 0xb96;
    public const uint GL_STENCIL_REF = 0xb97;
    public const uint GL_STENCIL_RENDERABLE = 0x8288;
    public const uint GL_STENCIL_TEST = 0xb90;
    public const uint GL_STENCIL_VALUE_MASK = 0xb93;
    public const uint GL_STENCIL_WRITEMASK = 0xb98;
    public const uint GL_STEREO = 0xc33;
    public const uint GL_STREAM_COPY = 0x88e2;
    public const uint GL_STREAM_DRAW = 0x88e0;
    public const uint GL_STREAM_READ = 0x88e1;
    public const uint GL_SUBPIXEL_BITS = 0xd50;
    public const uint GL_SYNC_CL_EVENT_ARB = 0x8240;
    public const uint GL_SYNC_CL_EVENT_COMPLETE_ARB = 0x8241;
    public const uint GL_SYNC_CONDITION = 0x9113;
    public const uint GL_SYNC_FENCE = 0x9116;
    public const uint GL_SYNC_FLAGS = 0x9115;
    public const uint GL_SYNC_FLUSH_COMMANDS_BIT = 0x1;
    public const uint GL_SYNC_GPU_COMMANDS_COMPLETE = 0x9117;
    public const uint GL_SYNC_STATUS = 0x9114;
    public const uint GL_T = 0x2001;
    public const uint GL_T2F_C3F_V3F = 0x2a2a;
    public const uint GL_T2F_C4F_N3F_V3F = 0x2a2c;
    public const uint GL_T2F_C4UB_V3F = 0x2a29;
    public const uint GL_T2F_N3F_V3F = 0x2a2b;
    public const uint GL_T2F_V3F = 0x2a27;
    public const uint GL_T4F_C4F_N3F_V4F = 0x2a2d;
    public const uint GL_T4F_V4F = 0x2a28;
    public const uint GL_TESS_CONTROL_OUTPUT_VERTICES = 0x8e75;
    public const uint GL_TESS_CONTROL_SHADER = 0x8e88;
    public const uint GL_TESS_CONTROL_SHADER_BIT = 0x8;
    public const uint GL_TESS_CONTROL_SHADER_PATCHES_ARB = 0x82f1;
    public const uint GL_TESS_CONTROL_SUBROUTINE = 0x92e9;
    public const uint GL_TESS_CONTROL_SUBROUTINE_UNIFORM = 0x92ef;
    public const uint GL_TESS_CONTROL_TEXTURE = 0x829c;
    public const uint GL_TESS_EVALUATION_SHADER = 0x8e87;
    public const uint GL_TESS_EVALUATION_SHADER_BIT = 0x10;
    public const uint GL_TESS_EVALUATION_SHADER_INVOCATIONS_ARB = 0x82f2;
    public const uint GL_TESS_EVALUATION_SUBROUTINE = 0x92ea;
    public const uint GL_TESS_EVALUATION_SUBROUTINE_UNIFORM = 0x92f0;
    public const uint GL_TESS_EVALUATION_TEXTURE = 0x829d;
    public const uint GL_TESS_GEN_MODE = 0x8e76;
    public const uint GL_TESS_GEN_POINT_MODE = 0x8e79;
    public const uint GL_TESS_GEN_SPACING = 0x8e77;
    public const uint GL_TESS_GEN_VERTEX_ORDER = 0x8e78;
    public const uint GL_TEXTURE = 0x1702;
    public const uint GL_TEXTURE0 = 0x84c0;
    public const uint GL_TEXTURE1 = 0x84c1;
    public const uint GL_TEXTURE10 = 0x84ca;
    public const uint GL_TEXTURE11 = 0x84cb;
    public const uint GL_TEXTURE12 = 0x84cc;
    public const uint GL_TEXTURE13 = 0x84cd;
    public const uint GL_TEXTURE14 = 0x84ce;
    public const uint GL_TEXTURE15 = 0x84cf;
    public const uint GL_TEXTURE16 = 0x84d0;
    public const uint GL_TEXTURE17 = 0x84d1;
    public const uint GL_TEXTURE18 = 0x84d2;
    public const uint GL_TEXTURE19 = 0x84d3;
    public const uint GL_TEXTURE2 = 0x84c2;
    public const uint GL_TEXTURE20 = 0x84d4;
    public const uint GL_TEXTURE21 = 0x84d5;
    public const uint GL_TEXTURE22 = 0x84d6;
    public const uint GL_TEXTURE23 = 0x84d7;
    public const uint GL_TEXTURE24 = 0x84d8;
    public const uint GL_TEXTURE25 = 0x84d9;
    public const uint GL_TEXTURE26 = 0x84da;
    public const uint GL_TEXTURE27 = 0x84db;
    public const uint GL_TEXTURE28 = 0x84dc;
    public const uint GL_TEXTURE29 = 0x84dd;
    public const uint GL_TEXTURE3 = 0x84c3;
    public const uint GL_TEXTURE30 = 0x84de;
    public const uint GL_TEXTURE31 = 0x84df;
    public const uint GL_TEXTURE4 = 0x84c4;
    public const uint GL_TEXTURE5 = 0x84c5;
    public const uint GL_TEXTURE6 = 0x84c6;
    public const uint GL_TEXTURE7 = 0x84c7;
    public const uint GL_TEXTURE8 = 0x84c8;
    public const uint GL_TEXTURE9 = 0x84c9;
    public const uint GL_TEXTURE_1D = 0xde0;
    public const uint GL_TEXTURE_1D_ARRAY = 0x8c18;
    public const uint GL_TEXTURE_2D = 0xde1;
    public const uint GL_TEXTURE_2D_ARRAY = 0x8c1a;
    public const uint GL_TEXTURE_2D_MULTISAMPLE = 0x9100;
    public const uint GL_TEXTURE_2D_MULTISAMPLE_ARRAY = 0x9102;
    public const uint GL_TEXTURE_3D = 0x806f;
    public const uint GL_TEXTURE_ALPHA_SIZE = 0x805f;
    public const uint GL_TEXTURE_ALPHA_TYPE = 0x8c13;
    public const uint GL_TEXTURE_BASE_LEVEL = 0x813c;
    public const uint GL_TEXTURE_BINDING_1D = 0x8068;
    public const uint GL_TEXTURE_BINDING_1D_ARRAY = 0x8c1c;
    public const uint GL_TEXTURE_BINDING_2D = 0x8069;
    public const uint GL_TEXTURE_BINDING_2D_ARRAY = 0x8c1d;
    public const uint GL_TEXTURE_BINDING_2D_MULTISAMPLE = 0x9104;
    public const uint GL_TEXTURE_BINDING_2D_MULTISAMPLE_ARRAY = 0x9105;
    public const uint GL_TEXTURE_BINDING_3D = 0x806a;
    public const uint GL_TEXTURE_BINDING_BUFFER = 0x8c2c;
    public const uint GL_TEXTURE_BINDING_CUBE_MAP = 0x8514;
    public const uint GL_TEXTURE_BINDING_CUBE_MAP_ARRAY = 0x900a;
    public const uint GL_TEXTURE_BINDING_CUBE_MAP_ARRAY_ARB = 0x900a;
    public const uint GL_TEXTURE_BINDING_RECTANGLE = 0x84f6;
    public const uint GL_TEXTURE_BIT = 0x40000;
    public const uint GL_TEXTURE_BLUE_SIZE = 0x805e;
    public const uint GL_TEXTURE_BLUE_TYPE = 0x8c12;
    public const uint GL_TEXTURE_BORDER = 0x1005;
    public const uint GL_TEXTURE_BORDER_COLOR = 0x1004;
    public const uint GL_TEXTURE_BUFFER = 0x8c2a;
    public const uint GL_TEXTURE_BUFFER_BINDING = 0x8c2a;
    public const uint GL_TEXTURE_BUFFER_DATA_STORE_BINDING = 0x8c2d;
    public const uint GL_TEXTURE_BUFFER_OFFSET = 0x919d;
    public const uint GL_TEXTURE_BUFFER_OFFSET_ALIGNMENT = 0x919f;
    public const uint GL_TEXTURE_BUFFER_SIZE = 0x919e;
    public const uint GL_TEXTURE_COMPARE_FUNC = 0x884d;
    public const uint GL_TEXTURE_COMPARE_MODE = 0x884c;
    public const uint GL_TEXTURE_COMPRESSED = 0x86a1;
    public const uint GL_TEXTURE_COMPRESSED_BLOCK_HEIGHT = 0x82b2;
    public const uint GL_TEXTURE_COMPRESSED_BLOCK_SIZE = 0x82b3;
    public const uint GL_TEXTURE_COMPRESSED_BLOCK_WIDTH = 0x82b1;
    public const uint GL_TEXTURE_COMPRESSED_IMAGE_SIZE = 0x86a0;
    public const uint GL_TEXTURE_COMPRESSION_HINT = 0x84ef;
    public const uint GL_TEXTURE_COORD_ARRAY = 0x8078;
    public const uint GL_TEXTURE_COORD_ARRAY_COUNT_EXT = 0x808b;
    public const uint GL_TEXTURE_COORD_ARRAY_EXT = 0x8078;
    public const uint GL_TEXTURE_COORD_ARRAY_POINTER = 0x8092;
    public const uint GL_TEXTURE_COORD_ARRAY_POINTER_EXT = 0x8092;
    public const uint GL_TEXTURE_COORD_ARRAY_SIZE = 0x8088;
    public const uint GL_TEXTURE_COORD_ARRAY_SIZE_EXT = 0x8088;
    public const uint GL_TEXTURE_COORD_ARRAY_STRIDE = 0x808a;
    public const uint GL_TEXTURE_COORD_ARRAY_STRIDE_EXT = 0x808a;
    public const uint GL_TEXTURE_COORD_ARRAY_TYPE = 0x8089;
    public const uint GL_TEXTURE_COORD_ARRAY_TYPE_EXT = 0x8089;
    public const uint GL_TEXTURE_CUBE_MAP = 0x8513;
    public const uint GL_TEXTURE_CUBE_MAP_ARRAY = 0x9009;
    public const uint GL_TEXTURE_CUBE_MAP_ARRAY_ARB = 0x9009;
    public const uint GL_TEXTURE_CUBE_MAP_NEGATIVE_X = 0x8516;
    public const uint GL_TEXTURE_CUBE_MAP_NEGATIVE_Y = 0x8518;
    public const uint GL_TEXTURE_CUBE_MAP_NEGATIVE_Z = 0x851a;
    public const uint GL_TEXTURE_CUBE_MAP_POSITIVE_X = 0x8515;
    public const uint GL_TEXTURE_CUBE_MAP_POSITIVE_Y = 0x8517;
    public const uint GL_TEXTURE_CUBE_MAP_POSITIVE_Z = 0x8519;
    public const uint GL_TEXTURE_CUBE_MAP_SEAMLESS = 0x884f;
    public const uint GL_TEXTURE_DEPTH = 0x8071;
    public const uint GL_TEXTURE_DEPTH_SIZE = 0x884a;
    public const uint GL_TEXTURE_DEPTH_TYPE = 0x8c16;
    public const uint GL_TEXTURE_ENV = 0x2300;
    public const uint GL_TEXTURE_ENV_COLOR = 0x2201;
    public const uint GL_TEXTURE_ENV_MODE = 0x2200;
    public const uint GL_TEXTURE_FETCH_BARRIER_BIT = 0x8;
    public const uint GL_TEXTURE_FIXED_SAMPLE_LOCATIONS = 0x9107;
    public const uint GL_TEXTURE_GATHER = 0x82a2;
    public const uint GL_TEXTURE_GATHER_SHADOW = 0x82a3;
    public const uint GL_TEXTURE_GEN_MODE = 0x2500;
    public const uint GL_TEXTURE_GEN_Q = 0xc63;
    public const uint GL_TEXTURE_GEN_R = 0xc62;
    public const uint GL_TEXTURE_GEN_S = 0xc60;
    public const uint GL_TEXTURE_GEN_T = 0xc61;
    public const uint GL_TEXTURE_GREEN_SIZE = 0x805d;
    public const uint GL_TEXTURE_GREEN_TYPE = 0x8c11;
    public const uint GL_TEXTURE_HEIGHT = 0x1001;
    public const uint GL_TEXTURE_IMAGE_FORMAT = 0x828f;
    public const uint GL_TEXTURE_IMAGE_TYPE = 0x8290;
    public const uint GL_TEXTURE_IMMUTABLE_FORMAT = 0x912f;
    public const uint GL_TEXTURE_IMMUTABLE_LEVELS = 0x82df;
    public const uint GL_TEXTURE_INTENSITY_SIZE = 0x8061;
    public const uint GL_TEXTURE_INTERNAL_FORMAT = 0x1003;
    public const uint GL_TEXTURE_LOD_BIAS = 0x8501;
    public const uint GL_TEXTURE_LUMINANCE_SIZE = 0x8060;
    public const uint GL_TEXTURE_MAG_FILTER = 0x2800;
    public const uint GL_TEXTURE_MATRIX = 0xba8;
    public const uint GL_TEXTURE_MAX_LEVEL = 0x813d;
    public const uint GL_TEXTURE_MAX_LOD = 0x813b;
    public const uint GL_TEXTURE_MIN_FILTER = 0x2801;
    public const uint GL_TEXTURE_MIN_LOD = 0x813a;
    public const uint GL_TEXTURE_PRIORITY = 0x8066;
    public const uint GL_TEXTURE_RECTANGLE = 0x84f5;
    public const uint GL_TEXTURE_RED_SIZE = 0x805c;
    public const uint GL_TEXTURE_RED_TYPE = 0x8c10;
    public const uint GL_TEXTURE_RESIDENT = 0x8067;
    public const uint GL_TEXTURE_SAMPLES = 0x9106;
    public const uint GL_TEXTURE_SHADOW = 0x82a1;
    public const uint GL_TEXTURE_SHARED_SIZE = 0x8c3f;
    public const uint GL_TEXTURE_SPARSE_ARB = 0x91a6;
    public const uint GL_TEXTURE_STACK_DEPTH = 0xba5;
    public const uint GL_TEXTURE_STENCIL_SIZE = 0x88f1;
    public const uint GL_TEXTURE_SWIZZLE_A = 0x8e45;
    public const uint GL_TEXTURE_SWIZZLE_B = 0x8e44;
    public const uint GL_TEXTURE_SWIZZLE_G = 0x8e43;
    public const uint GL_TEXTURE_SWIZZLE_R = 0x8e42;
    public const uint GL_TEXTURE_SWIZZLE_RGBA = 0x8e46;
    public const uint GL_TEXTURE_TARGET = 0x1006;
    public const uint GL_TEXTURE_UPDATE_BARRIER_BIT = 0x100;
    public const uint GL_TEXTURE_VIEW = 0x82b5;
    public const uint GL_TEXTURE_VIEW_MIN_LAYER = 0x82dd;
    public const uint GL_TEXTURE_VIEW_MIN_LEVEL = 0x82db;
    public const uint GL_TEXTURE_VIEW_NUM_LAYERS = 0x82de;
    public const uint GL_TEXTURE_VIEW_NUM_LEVELS = 0x82dc;
    public const uint GL_TEXTURE_WIDTH = 0x1000;
    public const uint GL_TEXTURE_WRAP_R = 0x8072;
    public const uint GL_TEXTURE_WRAP_S = 0x2802;
    public const uint GL_TEXTURE_WRAP_T = 0x2803;
    public const uint GL_TIMEOUT_EXPIRED = 0x911b;
    public const uint GL_TIMESTAMP = 0x8e28;
    public const uint GL_TIME_ELAPSED = 0x88bf;
    public const uint GL_TOP_LEVEL_ARRAY_SIZE = 0x930c;
    public const uint GL_TOP_LEVEL_ARRAY_STRIDE = 0x930d;
    public const uint GL_TRANSFORM_BIT = 0x1000;
    public const uint GL_TRANSFORM_FEEDBACK = 0x8e22;
    public const uint GL_TRANSFORM_FEEDBACK_ACTIVE = 0x8e24;
    public const uint GL_TRANSFORM_FEEDBACK_BARRIER_BIT = 0x800;
    public const uint GL_TRANSFORM_FEEDBACK_BINDING = 0x8e25;
    public const uint GL_TRANSFORM_FEEDBACK_BUFFER = 0x8c8e;
    public const uint GL_TRANSFORM_FEEDBACK_BUFFER_ACTIVE = 0x8e24;
    public const uint GL_TRANSFORM_FEEDBACK_BUFFER_BINDING = 0x8c8f;
    public const uint GL_TRANSFORM_FEEDBACK_BUFFER_INDEX = 0x934b;
    public const uint GL_TRANSFORM_FEEDBACK_BUFFER_MODE = 0x8c7f;
    public const uint GL_TRANSFORM_FEEDBACK_BUFFER_PAUSED = 0x8e23;
    public const uint GL_TRANSFORM_FEEDBACK_BUFFER_SIZE = 0x8c85;
    public const uint GL_TRANSFORM_FEEDBACK_BUFFER_START = 0x8c84;
    public const uint GL_TRANSFORM_FEEDBACK_BUFFER_STRIDE = 0x934c;
    public const uint GL_TRANSFORM_FEEDBACK_OVERFLOW_ARB = 0x82ec;
    public const uint GL_TRANSFORM_FEEDBACK_PAUSED = 0x8e23;
    public const uint GL_TRANSFORM_FEEDBACK_PRIMITIVES_WRITTEN = 0x8c88;
    public const uint GL_TRANSFORM_FEEDBACK_STREAM_OVERFLOW_ARB = 0x82ed;
    public const uint GL_TRANSFORM_FEEDBACK_VARYING = 0x92f4;
    public const uint GL_TRANSFORM_FEEDBACK_VARYINGS = 0x8c83;
    public const uint GL_TRANSFORM_FEEDBACK_VARYING_MAX_LENGTH = 0x8c76;
    public const uint GL_TRIANGLES = 0x4;
    public const uint GL_TRIANGLES_ADJACENCY = 0xc;
    public const uint GL_TRIANGLE_FAN = 0x6;
    public const uint GL_TRIANGLE_STRIP = 0x5;
    public const uint GL_TRIANGLE_STRIP_ADJACENCY = 0xd;
    public const uint GL_TRUE = 0x1;
    public const uint GL_TYPE = 0x92fa;
    public const uint GL_UNDEFINED_VERTEX = 0x8260;
    public const uint GL_UNIFORM = 0x92e1;
    public const uint GL_UNIFORM_ARRAY_STRIDE = 0x8a3c;
    public const uint GL_UNIFORM_ATOMIC_COUNTER_BUFFER_INDEX = 0x92da;
    public const uint GL_UNIFORM_BARRIER_BIT = 0x4;
    public const uint GL_UNIFORM_BLOCK = 0x92e2;
    public const uint GL_UNIFORM_BLOCK_ACTIVE_UNIFORMS = 0x8a42;
    public const uint GL_UNIFORM_BLOCK_ACTIVE_UNIFORM_INDICES = 0x8a43;
    public const uint GL_UNIFORM_BLOCK_BINDING = 0x8a3f;
    public const uint GL_UNIFORM_BLOCK_DATA_SIZE = 0x8a40;
    public const uint GL_UNIFORM_BLOCK_INDEX = 0x8a3a;
    public const uint GL_UNIFORM_BLOCK_NAME_LENGTH = 0x8a41;
    public const uint GL_UNIFORM_BLOCK_REFERENCED_BY_COMPUTE_SHADER = 0x90ec;
    public const uint GL_UNIFORM_BLOCK_REFERENCED_BY_FRAGMENT_SHADER = 0x8a46;
    public const uint GL_UNIFORM_BLOCK_REFERENCED_BY_GEOMETRY_SHADER = 0x8a45;
    public const uint GL_UNIFORM_BLOCK_REFERENCED_BY_TESS_CONTROL_SHADER = 0x84f0;
    public const uint GL_UNIFORM_BLOCK_REFERENCED_BY_TESS_EVALUATION_SHADER = 0x84f1;
    public const uint GL_UNIFORM_BLOCK_REFERENCED_BY_VERTEX_SHADER = 0x8a44;
    public const uint GL_UNIFORM_BUFFER = 0x8a11;
    public const uint GL_UNIFORM_BUFFER_BINDING = 0x8a28;
    public const uint GL_UNIFORM_BUFFER_OFFSET_ALIGNMENT = 0x8a34;
    public const uint GL_UNIFORM_BUFFER_SIZE = 0x8a2a;
    public const uint GL_UNIFORM_BUFFER_START = 0x8a29;
    public const uint GL_UNIFORM_IS_ROW_MAJOR = 0x8a3e;
    public const uint GL_UNIFORM_MATRIX_STRIDE = 0x8a3d;
    public const uint GL_UNIFORM_NAME_LENGTH = 0x8a39;
    public const uint GL_UNIFORM_OFFSET = 0x8a3b;
    public const uint GL_UNIFORM_SIZE = 0x8a38;
    public const uint GL_UNIFORM_TYPE = 0x8a37;
    public const uint GL_UNKNOWN_CONTEXT_RESET = 0x8255;
    public const uint GL_UNKNOWN_CONTEXT_RESET_ARB = 0x8255;
    public const uint GL_UNPACK_ALIGNMENT = 0xcf5;
    public const uint GL_UNPACK_COMPRESSED_BLOCK_DEPTH = 0x9129;
    public const uint GL_UNPACK_COMPRESSED_BLOCK_HEIGHT = 0x9128;
    public const uint GL_UNPACK_COMPRESSED_BLOCK_SIZE = 0x912a;
    public const uint GL_UNPACK_COMPRESSED_BLOCK_WIDTH = 0x9127;
    public const uint GL_UNPACK_IMAGE_HEIGHT = 0x806e;
    public const uint GL_UNPACK_LSB_FIRST = 0xcf1;
    public const uint GL_UNPACK_ROW_LENGTH = 0xcf2;
    public const uint GL_UNPACK_SKIP_IMAGES = 0x806d;
    public const uint GL_UNPACK_SKIP_PIXELS = 0xcf4;
    public const uint GL_UNPACK_SKIP_ROWS = 0xcf3;
    public const uint GL_UNPACK_SWAP_BYTES = 0xcf0;
    public const uint GL_UNSIGNALED = 0x9118;
    public const uint GL_UNSIGNED_BYTE = 0x1401;
    public const uint GL_UNSIGNED_BYTE_2_3_3_REV = 0x8362;
    public const uint GL_UNSIGNED_BYTE_3_3_2 = 0x8032;
    public const uint GL_UNSIGNED_INT = 0x1405;
    public const uint GL_UNSIGNED_INT64_ARB = 0x140f;
    public const uint GL_UNSIGNED_INT_10F_11F_11F_REV = 0x8c3b;
    public const uint GL_UNSIGNED_INT_10_10_10_2 = 0x8036;
    public const uint GL_UNSIGNED_INT_24_8 = 0x84fa;
    public const uint GL_UNSIGNED_INT_2_10_10_10_REV = 0x8368;
    public const uint GL_UNSIGNED_INT_5_9_9_9_REV = 0x8c3e;
    public const uint GL_UNSIGNED_INT_8_8_8_8 = 0x8035;
    public const uint GL_UNSIGNED_INT_8_8_8_8_REV = 0x8367;
    public const uint GL_UNSIGNED_INT_ATOMIC_COUNTER = 0x92db;
    public const uint GL_UNSIGNED_INT_IMAGE_1D = 0x9062;
    public const uint GL_UNSIGNED_INT_IMAGE_1D_ARRAY = 0x9068;
    public const uint GL_UNSIGNED_INT_IMAGE_2D = 0x9063;
    public const uint GL_UNSIGNED_INT_IMAGE_2D_ARRAY = 0x9069;
    public const uint GL_UNSIGNED_INT_IMAGE_2D_MULTISAMPLE = 0x906b;
    public const uint GL_UNSIGNED_INT_IMAGE_2D_MULTISAMPLE_ARRAY = 0x906c;
    public const uint GL_UNSIGNED_INT_IMAGE_2D_RECT = 0x9065;
    public const uint GL_UNSIGNED_INT_IMAGE_3D = 0x9064;
    public const uint GL_UNSIGNED_INT_IMAGE_BUFFER = 0x9067;
    public const uint GL_UNSIGNED_INT_IMAGE_CUBE = 0x9066;
    public const uint GL_UNSIGNED_INT_IMAGE_CUBE_MAP_ARRAY = 0x906a;
    public const uint GL_UNSIGNED_INT_SAMPLER_1D = 0x8dd1;
    public const uint GL_UNSIGNED_INT_SAMPLER_1D_ARRAY = 0x8dd6;
    public const uint GL_UNSIGNED_INT_SAMPLER_2D = 0x8dd2;
    public const uint GL_UNSIGNED_INT_SAMPLER_2D_ARRAY = 0x8dd7;
    public const uint GL_UNSIGNED_INT_SAMPLER_2D_MULTISAMPLE = 0x910a;
    public const uint GL_UNSIGNED_INT_SAMPLER_2D_MULTISAMPLE_ARRAY = 0x910d;
    public const uint GL_UNSIGNED_INT_SAMPLER_2D_RECT = 0x8dd5;
    public const uint GL_UNSIGNED_INT_SAMPLER_3D = 0x8dd3;
    public const uint GL_UNSIGNED_INT_SAMPLER_BUFFER = 0x8dd8;
    public const uint GL_UNSIGNED_INT_SAMPLER_CUBE = 0x8dd4;
    public const uint GL_UNSIGNED_INT_SAMPLER_CUBE_MAP_ARRAY = 0x900f;
    public const uint GL_UNSIGNED_INT_SAMPLER_CUBE_MAP_ARRAY_ARB = 0x900f;
    public const uint GL_UNSIGNED_INT_VEC2 = 0x8dc6;
    public const uint GL_UNSIGNED_INT_VEC3 = 0x8dc7;
    public const uint GL_UNSIGNED_INT_VEC4 = 0x8dc8;
    public const uint GL_UNSIGNED_NORMALIZED = 0x8c17;
    public const uint GL_UNSIGNED_SHORT = 0x1403;
    public const uint GL_UNSIGNED_SHORT_1_5_5_5_REV = 0x8366;
    public const uint GL_UNSIGNED_SHORT_4_4_4_4 = 0x8033;
    public const uint GL_UNSIGNED_SHORT_4_4_4_4_REV = 0x8365;
    public const uint GL_UNSIGNED_SHORT_5_5_5_1 = 0x8034;
    public const uint GL_UNSIGNED_SHORT_5_6_5 = 0x8363;
    public const uint GL_UNSIGNED_SHORT_5_6_5_REV = 0x8364;
    public const uint GL_UPPER_LEFT = 0x8ca2;
    public const uint GL_V2F = 0x2a20;
    public const uint GL_V3F = 0x2a21;
    public const uint GL_VALIDATE_STATUS = 0x8b83;
    public const uint GL_VENDOR = 0x1f00;
    public const uint GL_VERSION = 0x1f02;
    public const uint GL_VERSION_1_0 = 0x1;
    public const uint GL_VERSION_1_1 = 0x1;
    public const uint GL_VERSION_1_2 = 0x1;
    public const uint GL_VERSION_1_3 = 0x1;
    public const uint GL_VERSION_1_4 = 0x1;
    public const uint GL_VERSION_1_5 = 0x1;
    public const uint GL_VERSION_2_0 = 0x1;
    public const uint GL_VERSION_2_1 = 0x1;
    public const uint GL_VERSION_3_0 = 0x1;
    public const uint GL_VERSION_3_1 = 0x1;
    public const uint GL_VERSION_3_2 = 0x1;
    public const uint GL_VERSION_3_3 = 0x1;
    public const uint GL_VERSION_4_0 = 0x1;
    public const uint GL_VERSION_4_1 = 0x1;
    public const uint GL_VERSION_4_2 = 0x1;
    public const uint GL_VERSION_4_3 = 0x1;
    public const uint GL_VERSION_4_4 = 0x1;
    public const uint GL_VERSION_4_5 = 0x1;
    public const uint GL_VERTEX_ARRAY = 0x8074;
    public const uint GL_VERTEX_ARRAY_BINDING = 0x85b5;
    public const uint GL_VERTEX_ARRAY_COUNT_EXT = 0x807d;
    public const uint GL_VERTEX_ARRAY_EXT = 0x8074;
    public const uint GL_VERTEX_ARRAY_POINTER = 0x808e;
    public const uint GL_VERTEX_ARRAY_POINTER_EXT = 0x808e;
    public const uint GL_VERTEX_ARRAY_SIZE = 0x807a;
    public const uint GL_VERTEX_ARRAY_SIZE_EXT = 0x807a;
    public const uint GL_VERTEX_ARRAY_STRIDE = 0x807c;
    public const uint GL_VERTEX_ARRAY_STRIDE_EXT = 0x807c;
    public const uint GL_VERTEX_ARRAY_TYPE = 0x807b;
    public const uint GL_VERTEX_ARRAY_TYPE_EXT = 0x807b;
    public const uint GL_VERTEX_ATTRIB_ARRAY_BARRIER_BIT = 0x1;
    public const uint GL_VERTEX_ATTRIB_ARRAY_BUFFER_BINDING = 0x889f;
    public const uint GL_VERTEX_ATTRIB_ARRAY_DIVISOR = 0x88fe;
    public const uint GL_VERTEX_ATTRIB_ARRAY_ENABLED = 0x8622;
    public const uint GL_VERTEX_ATTRIB_ARRAY_INTEGER = 0x88fd;
    public const uint GL_VERTEX_ATTRIB_ARRAY_LONG = 0x874e;
    public const uint GL_VERTEX_ATTRIB_ARRAY_NORMALIZED = 0x886a;
    public const uint GL_VERTEX_ATTRIB_ARRAY_POINTER = 0x8645;
    public const uint GL_VERTEX_ATTRIB_ARRAY_SIZE = 0x8623;
    public const uint GL_VERTEX_ATTRIB_ARRAY_STRIDE = 0x8624;
    public const uint GL_VERTEX_ATTRIB_ARRAY_TYPE = 0x8625;
    public const uint GL_VERTEX_ATTRIB_BINDING = 0x82d4;
    public const uint GL_VERTEX_ATTRIB_RELATIVE_OFFSET = 0x82d5;
    public const uint GL_VERTEX_BINDING_BUFFER = 0x8f4f;
    public const uint GL_VERTEX_BINDING_DIVISOR = 0x82d6;
    public const uint GL_VERTEX_BINDING_OFFSET = 0x82d7;
    public const uint GL_VERTEX_BINDING_STRIDE = 0x82d8;
    public const uint GL_VERTEX_PROGRAM_POINT_SIZE = 0x8642;
    public const uint GL_VERTEX_SHADER = 0x8b31;
    public const uint GL_VERTEX_SHADER_BIT = 0x1;
    public const uint GL_VERTEX_SHADER_INVOCATIONS_ARB = 0x82f0;
    public const uint GL_VERTEX_SUBROUTINE = 0x92e8;
    public const uint GL_VERTEX_SUBROUTINE_UNIFORM = 0x92ee;
    public const uint GL_VERTEX_TEXTURE = 0x829b;
    public const uint GL_VERTICES_SUBMITTED_ARB = 0x82ee;
    public const uint GL_VIEWPORT = 0xba2;
    public const uint GL_VIEWPORT_BIT = 0x800;
    public const uint GL_VIEWPORT_BOUNDS_RANGE = 0x825d;
    public const uint GL_VIEWPORT_INDEX_PROVOKING_VERTEX = 0x825f;
    public const uint GL_VIEWPORT_SUBPIXEL_BITS = 0x825c;
    public const uint GL_VIEW_CLASS_128_BITS = 0x82c4;
    public const uint GL_VIEW_CLASS_16_BITS = 0x82ca;
    public const uint GL_VIEW_CLASS_24_BITS = 0x82c9;
    public const uint GL_VIEW_CLASS_32_BITS = 0x82c8;
    public const uint GL_VIEW_CLASS_48_BITS = 0x82c7;
    public const uint GL_VIEW_CLASS_64_BITS = 0x82c6;
    public const uint GL_VIEW_CLASS_8_BITS = 0x82cb;
    public const uint GL_VIEW_CLASS_96_BITS = 0x82c5;
    public const uint GL_VIEW_CLASS_BPTC_FLOAT = 0x82d3;
    public const uint GL_VIEW_CLASS_BPTC_UNORM = 0x82d2;
    public const uint GL_VIEW_CLASS_RGTC1_RED = 0x82d0;
    public const uint GL_VIEW_CLASS_RGTC2_RG = 0x82d1;
    public const uint GL_VIEW_CLASS_S3TC_DXT1_RGB = 0x82cc;
    public const uint GL_VIEW_CLASS_S3TC_DXT1_RGBA = 0x82cd;
    public const uint GL_VIEW_CLASS_S3TC_DXT3_RGBA = 0x82ce;
    public const uint GL_VIEW_CLASS_S3TC_DXT5_RGBA = 0x82cf;
    public const uint GL_VIEW_COMPATIBILITY_CLASS = 0x82b6;
    public const uint GL_VIRTUAL_PAGE_SIZE_INDEX_ARB = 0x91a7;
    public const uint GL_VIRTUAL_PAGE_SIZE_X_ARB = 0x9195;
    public const uint GL_VIRTUAL_PAGE_SIZE_Y_ARB = 0x9196;
    public const uint GL_VIRTUAL_PAGE_SIZE_Z_ARB = 0x9197;
    public const uint GL_WAIT_FAILED = 0x911d;
    public const uint GL_WRITE_ONLY = 0x88b9;
    public const uint GL_XOR = 0x1506;
    public const uint GL_ZERO = 0x0;
    public const uint GL_ZERO_TO_ONE = 0x935f;
    public const uint GL_ZOOM_X = 0xd16;
    public const uint GL_ZOOM_Y = 0xd17;

    #endregion

    // Описание функции glEnable
    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void glEnable(uint cap);

    // Описание функции glDisable
    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void glDisable(uint cap);

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void glLineWidth(GLfloat width);

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void glBlendFunc(GLenum sfactor, GLenum dfactor);

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void glDepthFunc(GLenum func);

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr glGetString(uint name);

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr glClear(uint mask);

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr glClearColor(float red, float green, float blue, float alpha);

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern IntPtr wglCreateContext(IntPtr hdc);

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern bool wglMakeCurrent(IntPtr hdc, IntPtr hglrc);

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern bool wglDeleteContext(IntPtr hglrc);

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern IntPtr wglSwapBuffers(IntPtr hdc);

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern IntPtr wglGetCurrentDC();

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern uint glGetError();

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern void glDrawArrays(uint mode, int first, int count);

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern void glDrawElements(GLenum mode, GLsizei count, GLenum type, IntPtr indices);

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    public static extern void glBindTexture(GLenum target, GLuint texture);

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void glTexImage2D(uint target, int level, int internalFormat, int width, int height,
      int border, uint format, uint type, IntPtr data);

    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void glTexParameteri(uint target, uint pname, int param);

    [DllImport("opengl32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
    public static extern IntPtr wglGetProcAddress(string functionName);

    // Импорт функции glReadPixels из библиотеки OpenGL
    [DllImport("opengl32.dll")]
    public static extern void glReadPixels(int x, int y, int width, int height, uint format, uint type, IntPtr pixels);

    // Импорт функции glViewport из библиотеки OpenGL
    [DllImport("opengl32.dll", CallingConvention = CallingConvention.Winapi)]
    public static extern void glViewport(int x, int y, int width, int height);

    public static void PrintGlError()
    {
      var error = glGetError();

      // Проверка наличия ошибки
      if (error != GL_NO_ERROR)
        // Обработка ошибки
        switch (error)
        {
          case GL_INVALID_ENUM:
            Console.WriteLine("OpenGL error: GL_INVALID_ENUM");
            break;
          case GL_INVALID_VALUE:
            Console.WriteLine("OpenGL error: GL_INVALID_VALUE");
            break;
          case GL_INVALID_OPERATION:
            Console.WriteLine("OpenGL error: GL_INVALID_OPERATION");
            break;
          case GL_OUT_OF_MEMORY:
            Console.WriteLine("OpenGL error: GL_OUT_OF_MEMORY");
            break;
          default:
            Console.WriteLine("OpenGL error: " + error);
            break;
        }
    }

    public static void glGenBuffers(int n, ref uint buffers)
    {
      var ptr = wglGetProcAddress("glGenBuffers");
      var glGenBuffersFunc = Marshal.GetDelegateForFunctionPointer<glGenBuffersDelegate>(ptr);
      glGenBuffersFunc(n, ref buffers);
    }

    public static void glCreateTextures(GLenum target, GLsizei n, ref GLuint textures)
    {
      var ptr = wglGetProcAddress("glCreateTextures");
      Marshal.GetDelegateForFunctionPointer<glCreateTexturesDelegate>(ptr)(target, n, ref textures);
    }

    public static void glDeleteTextures(GLsizei n, GLuint[] textures)
    {
      var ptr = wglGetProcAddress("glDeleteTextures");
      Marshal.GetDelegateForFunctionPointer<glDeleteTexturesDelegate>(ptr)(n, textures);
    }

    public static void glBindBuffer(uint type, uint bufferId)
    {
      var ptr = wglGetProcAddress("glBindBuffer");
      Marshal.GetDelegateForFunctionPointer<glBindBufferDelegate>(ptr)(type, bufferId);
    }

    public static void glBufferData(uint target, int size, IntPtr data, uint usage)
    {
      var ptr = wglGetProcAddress("glBufferData");
      Marshal.GetDelegateForFunctionPointer<glBufferDataDelegate>(ptr)(target, size, data, usage);
    }

    public static void glBufferData(uint target, float[] data, uint usage)
    {
      var sizeInBytes = data.Length * sizeof(float);
      var dataPointer = Marshal.AllocHGlobal(sizeInBytes);
      Marshal.Copy(data, 0, dataPointer, data.Length);
      glBufferData(target, sizeInBytes, dataPointer, usage);
      Marshal.FreeHGlobal(dataPointer);
    }

    public static void glTexSubImage2D(GLenum target, int level, int xOffset, int yOffset, int width, int height,
      GLenum format, GLenum type, IntPtr data)
    {
      var ptr = wglGetProcAddress("glTexSubImage2D");
      Marshal.GetDelegateForFunctionPointer<glTexSubImage2DDelegate>(ptr)(target, level, xOffset, yOffset, width,
        height, format, type, data);
    }

    public static void glBufferData(uint target, uint[] data, uint usage)
    {
      /*var sizeInBytes = data.Length * sizeof(uint);
      var dataPointer = Marshal.AllocHGlobal(sizeInBytes);
      Marshal.Copy(data, 0, dataPointer, data.Length);
      glBufferData(target, sizeInBytes, dataPointer, usage);
      Marshal.FreeHGlobal(dataPointer);*/

      var sizeInBytes = data.Length * sizeof(uint);
      var dataPointer = Marshal.UnsafeAddrOfPinnedArrayElement(data, 0);
      glBufferData(target, sizeInBytes, dataPointer, usage);
    }

    public static void glUniformMatrix4fv(GLint location, GLsizei count, bool transpose, float[] data)
    {
      GLboolean t = (byte)GL_FALSE;
      if (transpose) t = (byte)GL_TRUE;
      // var pointer = Marshal.UnsafeAddrOfPinnedArrayElement(data, 0);

      // Allocate
      var sizeInBytes = data.Length * sizeof(float);
      var dataPointer = Marshal.AllocHGlobal(sizeInBytes);
      Marshal.Copy(data, 0, dataPointer, data.Length);

      glUniformMatrix4fv(location, count, t, dataPointer);

      // Free
      Marshal.FreeHGlobal(dataPointer);
    }

    public static void glUniformMatrix4fv(GLint location, GLsizei count, GLboolean transpose, IntPtr data)
    {
      var ptr = wglGetProcAddress("glUniformMatrix4fv");
      Marshal.GetDelegateForFunctionPointer<glUniformMatrix4fvDelegate>(ptr)(location, count, transpose, data);
    }

    public static void glUniform1i(GLint location, GLint v0)
    {
      var ptr = wglGetProcAddress("glUniform1i");
      Marshal.GetDelegateForFunctionPointer<glTwoIntInt>(ptr)(location, v0);
    }

    public static void glUniform3f(GLint location, float x, float y, float z)
    {
      var ptr = wglGetProcAddress("glUniform3f");
      Marshal.GetDelegateForFunctionPointer<glUniform3fDelegate>(ptr)(location, x, y, z);
    }

    public static void glEnableVertexAttribArray(uint index)
    {
      var ptr = wglGetProcAddress("glEnableVertexAttribArray");
      Marshal.GetDelegateForFunctionPointer<glCreateShaderDelegate>(ptr)(index);
    }

    public static void glActiveTexture(GLenum index)
    {
      var ptr = wglGetProcAddress("glActiveTexture");
      Marshal.GetDelegateForFunctionPointer<glOneUint>(ptr)(index);
    }

    public static uint glCreateShader(GLenum shaderType)
    {
      var ptr = wglGetProcAddress("glCreateShader");
      return Marshal.GetDelegateForFunctionPointer<glCreateShaderDelegate>(ptr)(shaderType);
    }

    public static uint glCreateProgram()
    {
      var ptr = wglGetProcAddress("glCreateProgram");
      return Marshal.GetDelegateForFunctionPointer<glCreateProgramDelegate>(ptr)();
    }

    public static void glAttachShader(uint program, uint shader)
    {
      var ptr = wglGetProcAddress("glAttachShader");
      Marshal.GetDelegateForFunctionPointer<glAttachShaderDelegate>(ptr)(program, shader);
    }

    public static void glLinkProgram(uint program)
    {
      var ptr = wglGetProcAddress("glLinkProgram");
      Marshal.GetDelegateForFunctionPointer<glLinkProgramDelegate>(ptr)(program);
    }

    public static void glUseProgram(uint program)
    {
      var ptr = wglGetProcAddress("glUseProgram");
      Marshal.GetDelegateForFunctionPointer<glLinkProgramDelegate>(ptr)(program);
    }

    public static void glDeleteShader(uint shader)
    {
      var ptr = wglGetProcAddress("glDeleteShader");
      Marshal.GetDelegateForFunctionPointer<glLinkProgramDelegate>(ptr)(shader);
    }

    public static void glShaderSource(uint shader, int count, IntPtr stringPtr, IntPtr length)
    {
      var ptr = wglGetProcAddress("glShaderSource");
      Marshal.GetDelegateForFunctionPointer<glShaderSourceDelegate>(ptr)(shader, count, stringPtr, length);
    }

    public static void glShaderSource(uint shader, string shaderCode)
    {
      byte[][] byteArray = { Encoding.ASCII.GetBytes(shaderCode) };
      var ptrArray = new IntPtr[byteArray.Length];
      for (var i = 0; i < byteArray.Length; i++)
        ptrArray[i] = Marshal.UnsafeAddrOfPinnedArrayElement(byteArray[i], 0);
      var dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement(ptrArray, 0);

      glShaderSource(shader, 1, dataPtr, IntPtr.Zero);
    }

    public static void glCompileShader(uint shader)
    {
      var ptr = wglGetProcAddress("glCompileShader");
      Marshal.GetDelegateForFunctionPointer<glCompileShaderDelegate>(ptr)(shader);
    }

    public static void glGetShaderiv(uint shader, uint pname, out int parameters)
    {
      var ptr = wglGetProcAddress("glGetShaderiv");
      Marshal.GetDelegateForFunctionPointer<glGetShaderivDelegate>(ptr)(shader, pname, out parameters);
    }

    public static void glGetProgramiv(uint program, uint pname, out int param)
    {
      var ptr = wglGetProcAddress("glGetProgramiv");
      Marshal.GetDelegateForFunctionPointer<glGetProgramivDelegate>(ptr)(program, pname, out param);
    }

    public static string glGetShaderInfoLog(uint shader, int maxLength)
    {
      var ptr = wglGetProcAddress("glGetShaderInfoLog");

      var byteArray = new byte[maxLength];
      var dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement(byteArray, 0);

      Marshal.GetDelegateForFunctionPointer<glGetShaderInfoLogDelegate>(ptr)(shader, maxLength, out var length,
        dataPtr);
      var segment = new ArraySegment<byte>(byteArray, 0, length);

      return Encoding.UTF8.GetString(segment);
    }

    public static string glGetProgramInfoLog(uint shader, int maxLength)
    {
      var ptr = wglGetProcAddress("glGetProgramInfoLog");

      var byteArray = new byte[maxLength];
      var dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement(byteArray, 0);

      Marshal.GetDelegateForFunctionPointer<glGetShaderInfoLogDelegate>(ptr)(shader, maxLength, out var length,
        dataPtr);
      var segment = new ArraySegment<byte>(byteArray, 0, length);
      return Encoding.UTF8.GetString(segment);
    }

    public static void glVertexAttribPointer(uint index, int size, uint type, bool normalized, int stride,
      IntPtr pointer)
    {
      var ptr = wglGetProcAddress("glVertexAttribPointer");
      Marshal.GetDelegateForFunctionPointer<glVertexAttribPointerDelegate>(ptr)(index, size, type, normalized, stride,
        pointer);
    }

    public static void glVertexAttribIPointer(uint index, int size, uint type, int stride, IntPtr pointer)
    {
      var ptr = wglGetProcAddress("glVertexAttribIPointer");
      Marshal.GetDelegateForFunctionPointer<glVertexAttribIPointerDelegate>(ptr)(index, size, type, stride, pointer);
    }

    /*public static void glDrawArrays(uint mode, int first, int count)
    {
      var ptr = wglGetProcAddress("glDrawArrays");
      Marshal.GetDelegateForFunctionPointer<glDrawArraysDelegate>(ptr)(mode, first, count);
    }*/

    public static int glGetAttribLocation(uint program, string name)
    {
      var ptr = wglGetProcAddress("glGetAttribLocation");
      //var bytes = Encoding.ASCII.GetBytes(name);
      //var dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement(bytes, 0);

      var namePtr = Marshal.StringToHGlobalAnsi(name);
      var attribLocation = Marshal.GetDelegateForFunctionPointer<glGetAttribLocationDelegate>(ptr)(program, namePtr);
      Marshal.FreeHGlobal(namePtr);
      return attribLocation;
    }

    public static int glGetUniformLocation(uint program, string name)
    {
      var ptr = wglGetProcAddress("glGetUniformLocation");
      //var bytes = Encoding.ASCII.GetBytes(name);
      //var dataPtr = Marshal.UnsafeAddrOfPinnedArrayElement(bytes, 0);

      var namePtr = Marshal.StringToHGlobalAnsi(name);
      var uniformLocation = Marshal.GetDelegateForFunctionPointer<glGetAttribLocationDelegate>(ptr)(program, namePtr);
      Marshal.FreeHGlobal(namePtr);
      return uniformLocation;
    }

    public static IntPtr glMapBuffer(GLenum target, GLenum access)
    {
      var ptr = wglGetProcAddress("glMapBuffer");
      return Marshal.GetDelegateForFunctionPointer<glMapBufferDelegate>(ptr)(target, access);
    }

    public static GLboolean glUnmapBuffer(GLenum target)
    {
      var ptr = wglGetProcAddress("glUnmapBuffer");
      return Marshal.GetDelegateForFunctionPointer<glUnmapBufferDelegate>(ptr)(target);
    }

    public static void glGenerateMipmap(GLuint target)
    {
      var ptr = wglGetProcAddress("glGenerateMipmap");
      Marshal.GetDelegateForFunctionPointer<glOneUint>(ptr)(target);
    }

    /*public static void glDrawElements(GLenum mode, GLsizei count, GLenum type, IntPtr indices)
    {
      var ptr = wglGetProcAddress("glDrawElements");
      Marshal.GetDelegateForFunctionPointer<glDrawElementsDelegate>(ptr)(mode, count, type, indices);
    }*/

    private delegate void glOneUint(uint p);

    private delegate void glTwoIntInt(int p1, int p2);

    private delegate void glGenBuffersDelegate(int n, ref uint buffers);

    private delegate void glCreateTexturesDelegate(GLenum target, GLsizei n, ref GLuint buffers);

    private delegate void glDeleteTexturesDelegate(GLsizei n, GLuint[] buffers);

    private delegate void glBindBufferDelegate(uint type, uint bufferId);

    private delegate void glBufferDataDelegate(uint target, int size, IntPtr data, uint usage);

    private delegate void glTexSubImage2DDelegate(GLenum target, int level, int xOffset, int yOffset, int width,
      int height,
      GLenum format, GLenum type, IntPtr data);

    private delegate uint glCreateShaderDelegate(uint shaderType);

    private delegate void glShaderSourceDelegate(uint shader, int count, IntPtr stringPtr, IntPtr length);

    private delegate void glCompileShaderDelegate(uint shader);

    private delegate void glGetShaderivDelegate(uint shader, uint pname, out int parameters);

    private delegate void glGetShaderInfoLogDelegate(uint shader, int maxLength, out int length, IntPtr infoLog);

    private delegate uint glCreateProgramDelegate();

    private delegate void glAttachShaderDelegate(uint program, uint shader);

    private delegate void glLinkProgramDelegate(uint program);

    private delegate void glGetProgramivDelegate(uint program, uint pname, out int param);

    private delegate void glDrawArraysDelegate(uint mode, int first, int count);

    private delegate int glGetAttribLocationDelegate(uint program, IntPtr name);

    private delegate IntPtr glMapBufferDelegate(GLenum target, GLenum access);

    private delegate GLboolean glUnmapBufferDelegate(GLenum target);

    private delegate void glUniform3fDelegate(int location, float x, float y, float z);

    private delegate void glUniformMatrix4fvDelegate(int location, int count, byte transpose, IntPtr data);

    private delegate void glDrawElementsDelegate(GLenum mode, GLsizei count, GLenum type, IntPtr indices);

    private delegate void glVertexAttribPointerDelegate(uint index, int size, uint type, bool normalized, int stride,
      IntPtr pointer);

    private delegate void glVertexAttribIPointerDelegate(uint index, int size, uint type, int stride,
      IntPtr pointer);
  }
}