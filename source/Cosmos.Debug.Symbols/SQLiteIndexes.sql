-- Creating non-clustered index for FOREIGN KEY 'FK_AssemblyFileMethod'
CREATE INDEX [IX_FK_AssemblyFileMethod]
ON [Methods]
    ([AssemblyFileID]);    


-- Creating non-clustered index for FOREIGN KEY 'FK_DocumentMethod'
CREATE INDEX [IX_FK_DocumentMethod]
ON [Methods]
    ([DocumentID]);

-- Creating non-clustered index for FOREIGN KEY 'FK_LabelMethod'
CREATE INDEX [IX_FK_LabelMethod]
ON [Methods]
    ([LabelStartID]);

-- Creating non-clustered index for FOREIGN KEY 'FK_LabelMethodEnd'
CREATE INDEX [IX_FK_LabelMethodEnd]
ON [Methods]
    ([LabelEndID]);


-- Creating non-clustered index for FOREIGN KEY 'FK_MethodIlOpMethod'
CREATE INDEX [IX_FK_MethodIlOpMethod]
ON [MethodIlOps]
    ([MethodID]);
