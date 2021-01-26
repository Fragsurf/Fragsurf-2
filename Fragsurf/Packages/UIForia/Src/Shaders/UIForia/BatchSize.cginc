            
#if BATCH_SIZE_SMALL

    #define BATCH_SIZE 8

#elif BATCH_SIZE_MEDIUM

    #define BATCH_SIZE 16
    
#elif BATCH_SIZE_MEDIUM

    #define BATCH_SIZE 32
    
#elif BATCH_SIZE_HUGE
    
    #define BATCH_SIZE 64

#else
    
    #define BATCH_SIZE 128
        
#endif 