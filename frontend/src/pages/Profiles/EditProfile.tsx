import Box from "@material-ui/core/Box";
import EditPasswordHeader from "./components/EditPasswordHeader";
import EditProfileHeader from './components/EditProfileHeader';
import React from 'react';

export function EditProfile() {

    return (
        <Box paddingTop="30px" marginTop="50px" marginBottom="100px" justifyContent="center">
            <div>
                <EditProfileHeader />
                <EditPasswordHeader />
            </div>
        </Box>
  );
}
